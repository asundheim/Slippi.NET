using Slippi.NET.Console.Types;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using static Slippi.NET.Console.Types.ConnectionEventTypes;

namespace Slippi.NET.Console;

public class ConsoleConnection : Connection
{
    public const string NETWORK_MESSAGE = "HELO";
    private const int DEFAULT_CONNECTION_TIMEOUT_MS = 20_000;

    private readonly ConsoleConnectionDetails _defaultConnectionDetails = new ConsoleConnectionDetails()
    {
        ConsoleNick = "unknown",
        GameDataCursor = new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 },
        Version = string.Empty,
        ClientToken = 0,
        AutoReconnect = true,
    };

    private string _ipAddress;
    private int _port;
    private bool _isRealtime;
    private ConnectionStatus _connectionStatus = ConnectionStatus.Disconnected;
    private TcpClient? _tcpClient = null;
    private ConsoleConnectionDetails _options;
    private bool _shouldReconnect = false;

    public ConsoleConnection(string? consoleNick = null) : base()
    {
        _ipAddress = "0.0.0.0";
        _port = (int)Ports.Default;
        _isRealtime = false;
        _options = _defaultConnectionDetails with { ConsoleNick = consoleNick ?? "unknown" };
    }

    #region Connection

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
        return _options;
    }

    public override void Connect(string ip, int port, bool isRealtime = false, int timeout = DEFAULT_CONNECTION_TIMEOUT_MS)
    {
        _ipAddress = ip;
        _port = port;
        _isRealtime = isRealtime;

        ConnectOnPort(ip, port, timeout);
    }

    private void ConnectOnPort(string ip, int port, int timeout)
    {
        SetStatus(ConnectionStatus.Connecting);

        _ = Task.Run(() =>
        {
            ConsoleCommunication consoleComms = new ConsoleCommunication();
            _tcpClient = new TcpClient(hostname: ip, port: port) { ReceiveTimeout = timeout };

            Emit(new ConnectionEvent() { Event = CONNECT });
            _shouldReconnect = true;

            string commState = ConsoleCommunicationState.INITIAL;

            NetworkStream networkStream = _tcpClient.GetStream();

            byte[] buffer = new byte[1024];
            int bufferLength = 0;

            try
            {
                while (_tcpClient.Connected)
                {
                    bufferLength = networkStream.Socket.Receive(buffer);

                    if (commState == ConsoleCommunicationState.INITIAL)
                    {
                        commState = GetInitialCommState(buffer, bufferLength);
                        SetStatus(ConnectionStatus.Connected);
                    }

                    if (commState == ConsoleCommunicationState.LEGACY)
                    {
                        // If the first message received was not a handshake message, either we
                        // connected to an old Nintendont version or a relay instance
                        HandleReplayData(buffer.AsSpan().Slice(0, bufferLength).ToArray());
                        continue;
                    }

                    consoleComms.Receive(buffer.AsSpan().Slice(0, bufferLength).ToArray());
                    var messages = consoleComms.GetMessages();
                    foreach (var message in messages )
                    {
                        ProcessMessage(message);
                    }
                }
            }
            catch (Exception e)
            {
                System.Console.WriteLine(e);

                _tcpClient.Close();
                SetStatus(ConnectionStatus.Disconnected);
                Emit(new ConnectionEvent() { Event = ERROR, Args = e });
            }
        });
    }

    public override void Disconnect()
    {
        SetStatus(ConnectionStatus.Disconnected);

        if (_tcpClient is not null)
        {
            _tcpClient.Close();
        }
    }

    #endregion

    #region Utilities

    private void ProcessMessage(CommunicationMessage message)
    {
        Emit(new ConnectionEvent() { Event = MESSAGE, Args = message });

        switch (message.Type)
        {
            case CommunicationType.KeepAlive:
                {
                    // TODO: This is the jankiest shit ever but it will allow for relay connections not
                    // TODO: to time out as long as the main connection is still receving keep alive messages
                    // TODO: Need to figure out a better solution for this. There should be no need to have an
                    // TODO: active Wii connection for the relay connection to keep itself alive
                    // (5 years ago)

                    byte[] fakeKeepAlive = [.. Encoding.UTF8.GetBytes(NETWORK_MESSAGE), (byte)'\0'];
                    HandleReplayData(fakeKeepAlive);

                    break;
                }
            case CommunicationType.Replay:
                {
                    byte[] readPos = message.Payload.Pos!;
                    byte[] cursorBytes = (byte[])_options.GameDataCursor;
                    if (!(message.Payload.ForcePos ?? false) && !readPos.AsSpan().SequenceEqual(cursorBytes))
                    {
                        throw new Exception($"Position of received data is incorrect. Expected: {Encoding.UTF8.GetString(readPos)}, Received: ${Encoding.UTF8.GetString(cursorBytes)}");
                    }

                    if (message.Payload.ForcePos ?? false)
                    {
                        System.Console.WriteLine("Overflow occured in Nintendont, data has likely been skipped and replay corrupted. ");
                    }

                    _options.GameDataCursor = message.Payload.NextPos!;

                    var data = message.Payload.Data!;
                    HandleReplayData(data);

                    break;
                }
            case CommunicationType.Handshake:
                {
                    if (!string.IsNullOrEmpty(message.Payload.Nick))
                    {
                        _options.ConsoleNick = message.Payload.Nick;
                    }

                    Span<byte> tokenBuf = message.Payload.ClientToken!.AsSpan();
                    _options.ClientToken = BinaryPrimitives.ReadUInt32BigEndian(tokenBuf);

                    if (!string.IsNullOrEmpty(message.Payload.NintendontVersion))
                    {
                        _options.Version = message.Payload.NintendontVersion;
                    }

                    _options.GameDataCursor = message.Payload.Pos!;

                    Emit(new ConnectionEvent() { Event = HANDSHAKE, Args = _options });

                    break;
                }
            default:
                {
                    System.Console.WriteLine($"Unknown message type: {message.Type}");

                    break;
                }
        }
    }

    private void SetStatus(ConnectionStatus status)
    {
        if (_connectionStatus != status)
        {
            _connectionStatus = status;
            Emit(new ConnectionEvent() { Event = STATUS_CHANGE, Args = _connectionStatus });
        }
    }

    private void HandleReplayData(byte[] data)
    {
        Emit(new ConnectionEvent() { Event = DATA, Args = data });
    }

    private string GetInitialCommState(byte[] data, int length)
    {
        if (length < 13)
        {
            return ConsoleCommunicationState.LEGACY;
        }

        ReadOnlySpan<byte> openingBytes = [0x7b, 0x69, 0x04, 0x74, 0x79, 0x70, 0x65, 0x55, 0x01];

        Span<byte> dataStart = data.AsSpan().Slice(start: 4, length: 9);

        return dataStart.SequenceEqual(openingBytes) ? ConsoleCommunicationState.NORMAL : ConsoleCommunicationState.LEGACY;
    }

    #endregion

    public override void Dispose()
    {
        throw new NotImplementedException();
    }
}
