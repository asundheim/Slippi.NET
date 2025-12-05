using Newtonsoft.Json;
using Slippi.NET.Console.Types;
using Slippi.NET.Console.Utils;
using Slippi.NET.Melee.Types;
using Slippi.NET.Slp.Reader;
using Slippi.NET.Utils;
using System.Diagnostics;
using System.Text;

namespace Slippi.NET.Console;
public class DolphinConnection : Connection
{
    private static int _enetRef = 0;

    private string _ipAddress;
    private int _port;
    private ConnectionStatus _connectionStatus = ConnectionStatus.Disconnected;
    private int _gameCursor = 0;
    private string _nickname = "unknown";
    private string _version = string.Empty;
    private readonly CancellationTokenSource _cts = new CancellationTokenSource();
    private MenuEvent? _previousMenuEvent = null;

    private JsonSerializerSettings _serializerSettings = new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore };

    public DolphinConnection() : base()
    {
        if (_enetRef == 0)
        {
            DolphinENet.Initialize();
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

        int hr = DolphinENet.Connect(ip, (ushort)port);
        if (hr != 0)
        {
            throw new Exception("Connection failed.");
        }

        SetStatus(ConnectionStatus.Connecting);
        HandleConnect();
        
        _ = Task.Run(() =>
        {
            ENetClientLoop(_cts.Token);
        });
    }

    private void ENetClientLoop(CancellationToken cancellation)
    {
        const int maxBufferSize = 2048;

        bool disconnect = false;
        byte[] buffer = new byte[maxBufferSize];
        int bufferLength = buffer.Length;
        while (!disconnect && !cancellation.IsCancellationRequested)
        {
            bool polled = false;

            while (!polled)
            {
                int hr = DolphinENet.Read(15, ref bufferLength, buffer);
                if (hr < 0)
                {
                    disconnect = true;
                    Debug.WriteLine("Disconnect");
                    break;
                }
                else if (hr == 1 /* S_FALSE */)
                {
                    break;
                }

                polled = true;
                HandleMessage(buffer.AsSpan().Slice(0, bufferLength));
                bufferLength = maxBufferSize;
            }
        }

        HandleDisconnect();
    }

    /// <summary>
    /// Send the handshake request after the ENet connection is established.
    /// </summary>
    private void HandleConnect()
    {
        _gameCursor = 0;
        DolphinMessage message = new DolphinMessage()
        {
            Type = DolphinMessageTypes.CONNECT_REQUEST,
            GameCursor = _gameCursor,
        };

        string messageJson = JsonConvert.SerializeObject(message, Formatting.Indented, settings: _serializerSettings);
        messageJson = messageJson.Replace("\r", string.Empty);
        byte[] messageBytes = Encoding.ASCII.GetBytes(messageJson);

        DolphinENet.SendToPeer(messageBytes, messageBytes.Length);
    }

    private void HandleMessage(Span<byte> packet)
    {
        Span<byte> data = packet;
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
            HandleDisconnect();
            return;
        }

        switch (message.Type)
        {
            case DolphinMessageTypes.CONNECT_REPLY:
                SetStatus(ConnectionStatus.Connected);

                _gameCursor = message.GameCursor!.Value;
                _nickname = message.Nickname ?? "unknown";
                _version = message.Version ?? string.Empty;

                EmitOnHandshakeEvent(GetDetails());
                EmitOnConnectEvent();
                
                break;

            case DolphinMessageTypes.GAME_EVENT:
                if (message.Payload is null)
                {
                    HandleDisconnect();
                    return;
                }

                UpdateCursor(message);

                byte[] gameData = Convert.FromBase64String(message.Payload!);
                HandleReplayData(gameData);

                break;
            case DolphinMessageTypes.MENU_EVENT:
                byte[] menuData = Convert.FromBase64String(message.Payload!);
                HandleMenuEvent(menuData);
                break;
            case DolphinMessageTypes.START_GAME:
            case DolphinMessageTypes.END_GAME:
                UpdateCursor(message);

                break;

            default:
                Debug.WriteLine($"Unknown message type: {message.Type}");

                break;
        }
    }

    public override void HandleDisconnect()
    {
        DolphinENet.Disconnect();

        SetStatus(ConnectionStatus.Disconnected);
    }

    #endregion

    #region Utils

    private void HandleReplayData(byte[] data)
    {
        EmitOnDataEvent(data);
    }

    private void HandleMenuEvent(byte[] data)
    {
        BufferReader x = new BufferReader(data);
        MenuScene menu = MenuUtils.GetMenuScene(x.ReadUInt16(0x1));
        SubMenuScene subMenu = x.ReadUInt8(0x3D).SafeEnumCast<SubMenuScene>() ?? SubMenuScene.UNKNOWN;
        MenuOnlineMode? onlineMode = null;
        int frameCount = x.ReadInt32(0x39) ?? 0;
        bool readyToStart = false;
        Stage? stage = null;
        float? stageSelectX = null;
        float? stageSelectY = null;

        List<PlayerMenuState>? players = null;
        if (menu == MenuScene.CHARACTER_SELECT || menu == MenuScene.SLIPPI_ONLINE_CSS)
        {
            readyToStart = (x.ReadUInt8(0x23) ?? 0) == 1;
            players = [];
            for (int i = 0; i < 4; i++)
            {
                const int controllerStatusOffset = 0x25;
                const int cursorPosOffset = 0x3;
                const int selectedCharacterOffset = 0x29;
                const int coinOffset = 0x2D;

                players.Add(new PlayerMenuState()
                {
                    PlayerIndex = i,
                    ControllerStatus = x.ReadUInt8(controllerStatusOffset + i).SafeEnumCast<MenuControllerStatus>() ?? MenuControllerStatus.Unplugged,
                    CursorX = x.ReadFloat(cursorPosOffset + (i * 2)),
                    CursorY = x.ReadFloat(cursorPosOffset + (i * 2) + 4),
                    Character = (x.ReadUInt8(selectedCharacterOffset + i).SafeEnumCast<MenuCharacter>() ?? MenuCharacter.UNKNOWN_CHARACTER).ConvertMenuCharacter(),
                    CoinDown = x.ReadUInt8(coinOffset + i) == 2, // this seems to only work for VS (not online CSS)
                });
            }

            if (menu == MenuScene.SLIPPI_ONLINE_CSS)
            {
                byte? tag = x.ReadUInt8(0x40);
                subMenu = tag switch
                {
                    0x00 => SubMenuScene.ONLINE_CSS,
                    0x05 => SubMenuScene.NAME_ENTRY,
                    _ => subMenu
                };

                onlineMode = x.ReadUInt8(0x3E).SafeEnumCast<MenuOnlineMode>();
            }
        }
        else if (menu == MenuScene.STAGE_SELECT)
        {
            stage = x.ReadUInt8(0x24).SafeEnumCast<Stage>();
            stageSelectX = x.ReadFloat(0x31);
            stageSelectY = x.ReadFloat(0x35);
        }

        MenuEvent menuEvent = new MenuEvent()
        {
            FrameCount = frameCount,
            Menu = menu,
            SubMenu = subMenu,
            OnlineMode = onlineMode,
            PlayerStates = players,
            Stage = stage,
            StageSelectX = stageSelectX,
            StageSelectY = stageSelectY,
            ReadyToStart = readyToStart,
            RawEvent = data
        };

        EmitOnMenuEvent(menuEvent);
        if (_previousMenuEvent is not null && !(_previousMenuEvent with { FrameCount = menuEvent.FrameCount }).Is(menuEvent))
        {
            EmitOnNewMenuEvent(menuEvent);
        }

        _previousMenuEvent = menuEvent;
    }

    private void SetStatus(ConnectionStatus status)
    {
        // Don't fire the event if the status hasn't actually changed
        if (_connectionStatus != status)
        {
            _connectionStatus = status;
            EmitOnStatusChangeEvent(_connectionStatus);
        }
    }

    private void UpdateCursor(DolphinMessage message)
    {
        if (_gameCursor != message.GameCursor)
        {
            Exception e = new Exception($"Unexpected game data cursor. Expected: {_gameCursor} but got: {message.GameCursor}.");
            System.Console.WriteLine(e.Message);

            EmitOnErrorEvent(e);
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
            //ENet.Library.Deinitialize();
            int _ = DolphinENet.Uninitialize();
        }

        _cts.Cancel();
        _cts.Dispose();
    }

    #endregion
}
