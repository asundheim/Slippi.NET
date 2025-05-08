using Semver;
using Slippi.NET.Slp.Parser.Types;
using Slippi.NET.Types;
using Slippi.NET.Utils;

namespace Slippi.NET.Slp.Parser;

public class SlpParser
{
    private const int ITEM_SETTINGS_BIT_COUNT = 40;
    public const int MAX_ROLLBACK_FRAMES = 7;

    private FramesType _frames = [];
    private RollbackCounter _rollbackCounter = new RollbackCounter();
    private GameStart? _settings = null;
    private GameEnd? _gameEnd = null;
    private int? _latestFrameIndex = null;
    private bool _settingsComplete = false;
    private int _lastFinalizedFrame = (int)Frames.FIRST - 1;
    private SlpParserOptions _options;
    private GeckoCodeList? _geckoList = null;

    public SlpParser(SlpParserOptions options)
    {
        _options = options;
    }

    public event EventHandler<GameEnd>? OnGameEnd;
    public event EventHandler<FrameEntry>? OnFrame;
    public event EventHandler<FrameEntry>? OnFinalizedFrame;
    public event EventHandler<GameStart>? OnSettings;
    public event EventHandler<FrameEntry>? OnRollback;

    public void HandleCommand(Command command, object payload)
    {
        switch (command)
        {
            case Command.GAME_START:
                HandleGameStart((payload as GameStartPayload)!);
                break;
            case Command.FRAME_START:
                HandleFrameStart((payload as FrameStartPayload)!);
                break;
            case Command.POST_FRAME_UPDATE:
                // We need to handle the post frame update first since that
                // will finalize the settings object, before we fire the frame update
                HandlePostFrameUpdate((payload as PostFrameUpdatePayload)!);
                HandleFrameUpdate(command, (payload as EventPayload)!);
                break;
            case Command.PRE_FRAME_UPDATE:
                HandleFrameUpdate(command, (payload as PreFrameUpdatePayload)!);
                break;
            case Command.ITEM_UPDATE:
                HandleItemUpdate((payload as ItemUpdatePayload)!);
                break;
            case Command.FRAME_BOOKEND:
                HandleFrameBookend((payload as FrameBookendPayload)!);
                break;
            case Command.GAME_END:
                HandleGameEnd((payload as GameEndPayload)!);
                break;
            case Command.GECKO_LIST:
                HandleGeckoList((payload as GeckoListPayload)!);
                break;
            default:
                break;
        };
    }

    public void Reset()
    {
        _frames = [];
        _settings = null;
        _gameEnd = null;
        _latestFrameIndex = null;
        _settingsComplete = false;
        _lastFinalizedFrame = (int)Frames.FIRST - 1;
    }

    public int GetLatestFrameNumber()
    {
        return _latestFrameIndex ?? (int)Frames.FIRST - 1;
    }

    public int GetPlayableFrameCount()
    {
        if (_latestFrameIndex is null)
        {
            return 0;
        }

        return _latestFrameIndex < (int)Frames.FIRST_PLAYABLE ? 0 : _latestFrameIndex.Value - (int)Frames.FIRST_PLAYABLE;
    }

    public GameStart? GetSettings()
    {
        return _settingsComplete ? _settings : null;
    }

    public List<EnabledItemType>? GetItems()
    {
        if (_settings?.ItemSpawnBehavior == ItemSpawnType.OFF)
        {
            return null;
        }

        ulong? itemBitField = _settings?.EnabledItems;
        if (itemBitField is null)
        {
            return null;
        }

        List<EnabledItemType> enabledItems = [];
        for (int i = 0; i < ITEM_SETTINGS_BIT_COUNT; i++)
        {
            if ((itemBitField.Value << i & 1) != 0)
            {
                enabledItems.Add((EnabledItemType)(2 << i));
            }
        }

        return enabledItems;
    }

    public GameEnd? GetGameEnd() => _gameEnd;

    public FramesType GetFrames() => _frames;

    public RollbackFrames GetRollbackFrames()
    {
        return new RollbackFrames()
        {
            Frames = _rollbackCounter.GetFrames(),
            Count = _rollbackCounter.GetCount(),
            Lengths = _rollbackCounter.GetLengths()
        };
    }

    public FrameEntry? GetFrame(int num) => _frames.TryGetValue(num, out FrameEntry? frame) ? frame : null;

    public GeckoCodeList? GetGeckoList() => _geckoList;

    private void HandleGeckoList(GeckoListPayload payload)
    {
        _geckoList = payload.GeckoList;
    }

    private void HandleGameEnd(GameEndPayload payload)
    {
        // Finalize remaining frames if necessary
        if (_latestFrameIndex is not null && _latestFrameIndex != _lastFinalizedFrame)
        {
            FinalizeFrames(_latestFrameIndex.Value);
        }

        _gameEnd = payload.GameEnd;
        OnGameEnd?.Invoke(this, _gameEnd);
    }

    private void HandleGameStart(GameStartPayload payload)
    {
        _settings = payload.GameStart;
        List<Player> players = payload.GameStart.Players;
        _settings.Players = players.Where(p => p.Type is not null && p.Type != 3).ToList();

        // Check to see if the file was created after the sheik fix so we know
        // we don't have to process the first frame of the game for the full settings
        if (payload.GameStart.SlpVersion is not null && 
            SemVersion.Parse(payload.GameStart.SlpVersion).CompareSortOrderTo(SemVersion.Parse("1.6.0")) >= 0)
        {
            CompleteSettings();
        }
    }

    private void HandleFrameStart(FrameStartPayload payload)
    {
        int currentFrameNumber = payload.FrameStart.Frame!.Value;

        if (_frames.TryGetValue(currentFrameNumber, out FrameEntry? frame))
        {
            frame.Start = payload.FrameStart;
        }
        else
        {
            _frames[currentFrameNumber] = new FrameEntry() { Start = payload.FrameStart };
        }
    }

    private void HandlePostFrameUpdate(PostFrameUpdatePayload payload)
    {
        if (_settingsComplete)
        {
            return;
        }

        // Finish calculating settings
        if (payload.PostFrameUpdate.Frame!.Value <= (int)Frames.FIRST)
        {
            int playerIndex = payload.PostFrameUpdate.PlayerIndex!.Value;
            Dictionary<int, Player> playersByIndex = _settings!.Players.ToDictionary(k => k.PlayerIndex, v => v);

            switch (payload.PostFrameUpdate.InternalCharacterId)
            {
                case 0x7:
                    playersByIndex[playerIndex].CharacterId = 0x13; // Sheik
                    break;
                case 0x13:
                    playersByIndex[playerIndex].CharacterId = 0x12; // Zelda
                    break;
                default:
                    break;
            }
        }

        if (payload.PostFrameUpdate.Frame!.Value > (int)Frames.FIRST)
        {
            CompleteSettings();
        }
    }

    private void HandleFrameUpdate(Command command, EventPayload payload)
    {
        string location = command == Command.PRE_FRAME_UPDATE ? "pre" : "post";
        FrameUpdate frameUpdate = payload switch
        {
            PreFrameUpdatePayload preFrameUpdatePayload => preFrameUpdatePayload.PreFrameUpdate,
            PostFrameUpdatePayload postFrameUpdatePayload => postFrameUpdatePayload.PostFrameUpdate,
            _ => throw new Exception("Unexpected payload type")
        };

        string field = (frameUpdate.IsFollower ?? false) ? "followers" : "player";
        int currentFrameNumber = frameUpdate.Frame!.Value;
        _latestFrameIndex = currentFrameNumber;

        if (location == "pre" && !(frameUpdate.IsFollower ?? false))
        {
            if (_frames.TryGetValue(currentFrameNumber, out FrameEntry? currentFrameEntry))
            {
                bool wasRolledBack = _rollbackCounter.CheckIfRollbackFrame(currentFrameEntry, frameUpdate.PlayerIndex!.Value);
                if (wasRolledBack)
                {
                    // frame is about to be overwritten
                    OnRollback?.Invoke(this, currentFrameEntry);
                }
            }
        }

        // ew
        if (_frames.TryGetValue(currentFrameNumber, out FrameEntry? currentFrame))
        {
            if (field == "followers")
            {
                currentFrame.Followers ??= [];
                if (currentFrame.Followers.TryGetValue(frameUpdate.PlayerIndex!.Value, out PlayerFrameData? playerFrameData))
                {
                    if (location == "pre")
                    {
                        playerFrameData!.Pre = (frameUpdate as PreFrameUpdate)!;
                    }
                    else
                    {
                        playerFrameData!.Post = (frameUpdate as PostFrameUpdate)!;
                    }
                }
                else
                {
                    if (location == "pre")
                    {
                        currentFrame.Followers[frameUpdate.PlayerIndex!.Value] = new PlayerFrameData()
                        {
                            Pre = (frameUpdate as PreFrameUpdate)!
                        };
                    }
                    else
                    {
                        currentFrame.Followers[frameUpdate.PlayerIndex!.Value] = new PlayerFrameData()
                        {
                            Post = (frameUpdate as PostFrameUpdate)!
                        };
                    }
                }
            }
            else
            {
                currentFrame.Players ??= [];
                if (currentFrame.Players.TryGetValue(frameUpdate.PlayerIndex!.Value, out PlayerFrameData? playerFrameData))
                {
                    if (location == "pre")
                    {
                        playerFrameData!.Pre = (frameUpdate as PreFrameUpdate)!;
                    }
                    else
                    {
                        playerFrameData!.Post = (frameUpdate as PostFrameUpdate)!;
                    }
                }
                else
                {
                    if (location == "pre")
                    {
                        currentFrame.Players[frameUpdate.PlayerIndex!.Value] = new PlayerFrameData()
                        {
                            Pre = (frameUpdate as PreFrameUpdate)!
                        };
                    }
                    else
                    {
                        currentFrame.Players[frameUpdate.PlayerIndex!.Value] = new PlayerFrameData()
                        {
                            Post = (frameUpdate as PostFrameUpdate)!
                        };
                    }
                }
            }
        }
        else
        {
            FrameEntry newFrame = new FrameEntry();
            if (field == "followers")
            {
                newFrame.Followers = [];
                if (location == "pre")
                {
                    newFrame.Followers[frameUpdate.PlayerIndex!.Value] = new PlayerFrameData()
                    {
                        Pre = (frameUpdate as PreFrameUpdate)!
                    };
                }
                else
                {
                    newFrame.Followers[frameUpdate.PlayerIndex!.Value] = new PlayerFrameData()
                    {
                        Post = (frameUpdate as PostFrameUpdate)!
                    };
                }
            }
            else
            {
                newFrame.Players ??= [];
                if (location == "pre")
                {
                    newFrame.Players[frameUpdate.PlayerIndex!.Value] = new PlayerFrameData()
                    {
                        Pre = (frameUpdate as PreFrameUpdate)!
                    };
                }
                else
                {
                    newFrame.Players[frameUpdate.PlayerIndex!.Value] = new PlayerFrameData()
                    {
                        Post = (frameUpdate as PostFrameUpdate)!
                    };
                }
            }
        }

        // If file is from before frame bookending, add frame to stats computer here. Does a little
        // more processing than necessary, but it works
        GameStart? settings = GetSettings();
        if (settings is not null && 
            (settings.SlpVersion is null || SemVersion.Parse(settings.SlpVersion).CompareSortOrderTo(SemVersion.Parse("2.2.0")) <= 0))
        {
            OnFrame?.Invoke(this, _frames[currentFrameNumber]);
            // Finalize the previous frame since no bookending exists
            FinalizeFrames(currentFrameNumber - 1);
        }
        else
        {
            // set(this.frames, [currentFrameNumber, "isTransferComplete"], false); ??? seems unused
        }
    }

    private void HandleItemUpdate(ItemUpdatePayload payload)
    {
        int currentFrameNumber = payload.ItemUpdate.Frame!.Value;
        if (_frames.TryGetValue(currentFrameNumber, out FrameEntry? frame))
        {
            frame.Items ??= [];
            frame.Items.Add(payload.ItemUpdate);
        }
        else
        {
            _frames[currentFrameNumber] = new FrameEntry()
            {
                Items = [payload.ItemUpdate]
            };
        }
    }

    private void HandleFrameBookend(FrameBookendPayload payload)
    {
        int latestFinalizedFrame = payload.FrameBookend.LatestFinalizedFrame!.Value;
        int currentFrameNumber = payload.FrameBookend.Frame!.Value;
        
        if (!_frames.TryGetValue(currentFrameNumber, out FrameEntry? frame))
        {
            _frames[currentFrameNumber] = new FrameEntry();
        }

        // Fire off a normal frame event
        OnFrame?.Invoke(this, _frames[currentFrameNumber]);

        // Finalize frames if necessary
        bool validLatestFrame = _settings?.GameMode == GameMode.ONLINE;
        if (validLatestFrame && latestFinalizedFrame >= (int)Frames.FIRST)
        {
            // Ensure valid latestFinalizedFrame
            if (_options.Strict && latestFinalizedFrame < currentFrameNumber - MAX_ROLLBACK_FRAMES)
            {
                throw new Exception($"latestFinalizedFrame should be within {MAX_ROLLBACK_FRAMES} frames of ${currentFrameNumber}");
            }

            FinalizeFrames(latestFinalizedFrame);
        }
        else
        {
            FinalizeFrames(currentFrameNumber - MAX_ROLLBACK_FRAMES);
        }
    }

    private void FinalizeFrames(int num)
    {
        while (_lastFinalizedFrame < num)
        {
            int frameToFinalize = _lastFinalizedFrame + 1;
            FrameEntry frame = GetFrame(frameToFinalize)!;

            // Check that we have all the pre and post frame data for all players if we're in strict mode
            if (_options.Strict)
            {
                foreach (var player in _settings!.Players)
                {
                    PlayerFrameData? playerFrameData = null;
                    frame.Players?.TryGetValue(player.PlayerIndex, out playerFrameData);

                    // Allow player frame info to be empty in non 1v1 games since
                    // players which have been defeated will have no frame info.
                    if (_settings!.Players.Count > 2 && playerFrameData is null)
                    {
                        continue;
                    }

                    if (playerFrameData is null || playerFrameData.Pre is null || playerFrameData.Post is null)
                    {
                        if (playerFrameData is null)
                        {
                            throw new Exception($"Could not finalize frame {frameToFinalize} of {num}: missing PlayerFrameData for player ${player.PlayerIndex}");
                        }
                        else
                        {
                            string preOrPost = playerFrameData.Pre is null ? "pre" : "post";
                            throw new Exception($"Could not finalize frame {frameToFinalize} of {num}: missing {preOrPost}-frame update for player {player.PlayerIndex}");
                        }
                    }
                }
            }

            // Our frame is complete so finalize the frame
            OnFinalizedFrame?.Invoke(this, frame);
            _lastFinalizedFrame = frameToFinalize;
        }
    }

    private void CompleteSettings()
    {
        if (!_settingsComplete)
        {
            _settingsComplete = true;
            OnSettings?.Invoke(this, _settings!);
        }
    }
}
