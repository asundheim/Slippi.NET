using ENet;
using Newtonsoft.Json;
using Slippi.NET.Console.Types;
using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slippi.NET.Console;
public class DolphinConnection : Connection
{
    private static int _enetRef = 0;
    private const int MAX_PEERS = 32;

    private string _ipAddress;
    private int _port;
    private ConnectionStatus _connectionStatus = ConnectionStatus.Disconnected;
    private int _gameCursor = 0;
    private string _nickname = "unknown";
    private string _version = string.Empty;
    private Peer? _peer = null;
    private Host? _client = null;
    private CancellationTokenSource _cts = new CancellationTokenSource();

    private JsonSerializerSettings _serializerSettings = new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore };


    public DolphinConnection() : base()
    {
        if (_enetRef == 0)
        {
            ENet.Library.Initialize();
        }

        Interlocked.Increment(ref _enetRef);

        _ipAddress = "0.0.0.0";
        _port = (int)Ports.Default;
    }

    public override ConnectionStatus GetStatus()
    {
        return _connectionStatus;
    }
    public override ConnectionSettings GetSettings()
    {
        return new ConnectionSettings()
        {
            IpAddress = _ipAddress,
            Port = _port,
        };
    }
    public override ConnectionDetails GetDetails()
    {
        return new ConnectionDetails()
        {
            ConsoleNick = _nickname,
            GameDataCursor = _gameCursor,
            Version = _version,
        };
    }

    #region Connection

    public override void Connect(string ip, int port, bool isRealtime, int timeout)
    {
        System.Console.WriteLine($"Connecting to {ip}:{port}");

        _ipAddress = ip;
        _port = port;

        _client = new Host();
        _client.Create(peerLimit: MAX_PEERS, channelLimit: 3, incomingBandwidth: 0, outgoingBandwidth: 0);

        Address address = new Address()
        {
            Port = (ushort)_port
        };
        address.SetHost(_ipAddress);

        SetStatus(ConnectionStatus.Connecting);

        _ = Task.Run(() =>
        {
            _peer = _client.Connect(address, channelLimit: 3, data: 1337);
            _peer.Value.Ping();

            Emit(new ConnectionEvent() { Event = ConnectionEventTypes.CONNECT });
            SetStatus(ConnectionStatus.Connected);

            ENetLoop(_cts.Token);
        });
    }

    private void ENetLoop(CancellationToken cancellation)
    {
        Event netEvent;
        bool disconnect = false;
        if (_client is not null)
        {
            while (!disconnect && !cancellation.IsCancellationRequested)
            {
                bool polled = false;

                while (!polled)
                {
                    if (_client.CheckEvents(out netEvent) <= 0)
                    {
                        if (_client.Service(timeout: 15, out netEvent) <= 0)
                        {
                            break;
                        }

                        polled = true;
                    }

                    switch (netEvent.Type)
                    {
                        case EventType.None:
                        case EventType.Timeout:
                            break;

                        case EventType.Connect:
                            OnConnect();

                            break;

                        case EventType.Disconnect:
                            Disconnect();
                            disconnect = true;

                            break;

                        case EventType.Receive:
                            System.Console.WriteLine("Packet received from server - Channel ID: " + netEvent.ChannelID + ", Data length: " + netEvent.Packet.Length);

                            OnMessage(netEvent.Packet);

                            netEvent.Packet.Dispose();

                            break;
                    }
                }
            }

            _client.Flush();
        }
    }

    private void OnConnect()
    {
        if (_peer is null)
        {
            System.Console.WriteLine("_peer is null!");
            return;
        }

        _gameCursor = 0;
        DolphinMessage message = new DolphinMessage()
        {
            Type = DolphinMessageType.CONNECT_REQUEST,
            GameCursor = _gameCursor,
        };

        string messageJson = JsonConvert.SerializeObject(message, Formatting.None, settings: _serializerSettings);
        byte[] messageBytes = Encoding.ASCII.GetBytes(messageJson);

        Packet packet = new Packet();
        packet.Create(messageBytes, messageBytes.Length, PacketFlags.Reliable);


        _peer.Value.Send(0, ref packet);
    }

    private void OnMessage(Packet packet)
    {
        Span<byte> data = stackalloc byte[packet.Length];
        string jsonString = Encoding.ASCII.GetString(data);

        DolphinMessage? message = JsonConvert.DeserializeObject<DolphinMessage>(jsonString, _serializerSettings);
        if (message is null)
        {
            System.Console.WriteLine($"Failed to decode packet into DolphinMessage:");
            System.Console.WriteLine(jsonString);

            return;
        }

        if (message.DolphinClosed == true)
        {
            Disconnect();
            return;
        }

        switch (message.Type) 
        {
            case DolphinMessageType.CONNECT_REPLY:
                _connectionStatus = ConnectionStatus.Connected;
                _gameCursor = message.GameCursor!.Value;
                _nickname = message.Nickname ?? "unknown";
                _version = message.Version ?? string.Empty;

                Emit(new ConnectionEvent() { Event = ConnectionEventTypes.HANDSHAKE, Args = GetDetails() });

                break;

            case DolphinMessageType.GAME_EVENT:
                if (message.Payload is null)
                {
                    Disconnect();
                    return;
                }

                UpdateCursor(message);

                byte[] gameData = Convert.FromBase64String(message.Payload!);
                HandleReplayData(gameData);

                break;

            case DolphinMessageType.START_GAME:
            case DolphinMessageType.END_GAME:
                UpdateCursor(message);

                break;

            default:
                System.Console.WriteLine($"Unknown message type: {message.Type}");

                break;
        }
    }

    public override void Disconnect()
    {
        if (_peer is not null)
        {
            _peer.Value.Disconnect(0);
            _peer = null;
        }

        if (_client is not null)
        {
            _client.Dispose();
            _client = null;
        }

        SetStatus(ConnectionStatus.Disconnected);
    }

    #endregion

    #region Utils

    private void HandleReplayData(byte[] data)
    {
        Emit(new ConnectionEvent() { Event = ConnectionEventTypes.DATA, Args = data });
    }

    private void SetStatus(ConnectionStatus status)
    {
        // Don't fire the event if the status hasn't actually changed
        if (_connectionStatus != status)
        {
            _connectionStatus = status;
            Emit(new ConnectionEvent() { Event = ConnectionEventTypes.STATUS_CHANGE, Args = _connectionStatus });
        }
    }

    private void UpdateCursor(DolphinMessage message)
    {
        if (_gameCursor != message.GameCursor)
        {
            Exception e = new Exception($"Unexpected game data cursor. Expected: {_gameCursor} but got: {message.GameCursor}.");
            System.Console.WriteLine(e.Message);

            Emit(new ConnectionEvent() { Event = ConnectionEventTypes.ERROR, Args = e });
        }

        _gameCursor = message.NextCursor!.Value;
    }

    #endregion

    #region IDisposable

    public override void Dispose()
    {
        int newEnetRef = Interlocked.Decrement(ref _enetRef);
        if (newEnetRef == 0)
        {
            ENet.Library.Deinitialize();
        }

        _cts.Cancel();
        _cts.Dispose();


    }

    #endregion
}
