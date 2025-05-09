using Slippi.NET.Slp;
using Slippi.NET.Slp.Parser;
using Slippi.NET.Slp.Parser.Types;
using Slippi.NET.Slp.Reader;
using Slippi.NET.Slp.Reader.Buffer;
using Slippi.NET.Slp.Reader.File;
using Slippi.NET.Slp.Reader.Types;
using Slippi.NET.Stats;
using Slippi.NET.Stats.Types;
using Slippi.NET.Types;
using Slippi.NET.Utils;   

namespace Slippi.NET;

public class SlippiGame : IDisposable
{
    private readonly SlpReadInput _input;
    private Metadata? _metadata = null;
    private StatsType? _finalStats = null;
    private readonly SlpParser _parser;
    private int? _readPosition = null;

    private readonly ActionsComputer _actionsComputer = new ActionsComputer();
    private readonly ConversionsComputer _conversionsComputer = new ConversionsComputer();
    private readonly ComboComputer _comboComputer = new ComboComputer();
    private readonly StockComputer _stockComputer = new StockComputer();
    private readonly InputComputer _inputComputer = new InputComputer();
    private readonly TargetBreakComputer _targetBreakComputer = new TargetBreakComputer();

    protected readonly StatsComputer _statsComputer;

    public SlippiGame(string filePath, StatOptions options)
    {
        _input = new SlpFileReadInput()
        {
            FilePath = filePath,
        };

        _statsComputer = new StatsComputer(options);
        _statsComputer.Register(
            _actionsComputer,
            _comboComputer,
            _conversionsComputer,
            _inputComputer,
            _stockComputer,
            _targetBreakComputer
        );

        _parser = new SlpParser(new SlpParserOptions());
        _parser.OnSettings += OnParserSettings;

        // Use finalized frames for stats computation
        _parser.OnFinalizedFrame += OnParserFinalizedFrame;
    }

    public SlippiGame(byte[] fileBytes, StatOptions options)
    {
        _input = new SlpBufferReadInput()
        {
            Buffer = fileBytes
        };

        _statsComputer = new StatsComputer(options);
        _statsComputer.Register(
            _actionsComputer,
            _comboComputer,
            _conversionsComputer,
            _inputComputer,
            _stockComputer,
            _targetBreakComputer
        );

        _parser = new SlpParser(new SlpParserOptions());
        _parser.OnSettings += OnParserSettings;

        // Use finalized frames for stats computation
        _parser.OnFinalizedFrame += OnParserFinalizedFrame;
    }

    public GameStart? GetSettings()
    {
        Process((_, _, _) => _parser.GetSettings() is not null);
        return _parser.GetSettings();
    }

    public IList<EnabledItemType>? GetItems()
    {
        Process();
        return _parser.GetItems();
    }

    public FrameEntry? GetLatestFrame()
    {
        Process();
        return _parser.GetLatestFrame();
    }

    public GameEnd? GetGameEnd(bool skipProcessing = false)
    {
        if (skipProcessing)
        {
            // Read game end block directly
            using SlpFile slpFile = SlpReader.OpenSlpFile(_input);
            GameEnd? gameEnd = slpFile.GetGameEnd();

            return gameEnd;
        }

        Process();
        return _parser.GetGameEnd();
    }

    public FramesType GetFrames()
    {
        Process();
        return _parser.GetFrames();
    }

    public RollbackFrames GetRollbackFrames()
    {
        Process();
        return _parser.GetRollbackFrames();
    }

    public GeckoCodeList? GetGeckoList()
    {
        Process((_, _, _) => _parser.GetGeckoList() is not null);
        return _parser.GetGeckoList();
    }

    public StatsType? GetStats()
    {
        if (_finalStats is not null)
        {
            return _finalStats;
        }

        Process();

        GameStart? settings = _parser.GetSettings();
        if (settings is null)
        {
            return null;
        }

        // Finish processing if we're not up to date
        _statsComputer.Process();
        IList<PlayerInput> inputs = _inputComputer.Fetch();
        IList<Stock> stocks = _stockComputer.Fetch();
        IList<Conversion> conversions = _conversionsComputer.Fetch();
        int playableFrameCount = _parser.GetPlayableFrameCount();
        IList<OverallType> overall = OverallStats.GenerateOverallStats(settings, inputs, conversions, playableFrameCount);

        GameEnd? gameEnd = _parser.GetGameEnd();
        bool gameComplete = gameEnd is not null;

        StatsType stats = new StatsType()
        {
            LastFrame = _parser.GetLatestFrameNumber(),
            PlayableFrameCount = playableFrameCount,
            Stocks = stocks,
            Conversions = conversions,
            Combos = _comboComputer.Fetch(),
            ActionCounts = _actionsComputer.Fetch(),
            Overall = overall,
            GameComplete = gameComplete,
        };

        if (gameComplete)
        {
            // If the game is complete, store a cached version of stats because it should not
            // change anymore. Ideally the statsCompuer.process and fetch functions would simply do no
            // work in this case instead but currently the conversions fetch function,
            // generateOverallStats, and maybe more are doing work on every call.
            _finalStats = stats;
        }

        return stats;
    }

    public StadiumStats? GetStadiumStats()
    {
        Process();

        GameStart? settings = _parser.GetSettings();
        if (settings is null)
        {
            return null;
        }

        FrameEntry? latestFrame = _parser.GetLatestFrame();
        if (latestFrame is null)
        {
            return null;
        }

        Dictionary<int, PlayerFrameData?>? players = latestFrame.Players;
        if (players is null)
        {
            return null;
        }

        _statsComputer.Process();

        switch (settings.GameMode)
        {
            case GameMode.TARGET_TEST:
                {
                    return new TargetTestStats()
                    {
                        TargetBreaks = _targetBreakComputer.Fetch()
                    };
                }
            case GameMode.HOME_RUN_CONTEST:
                {
                    HomeRunDistanceInfo? distanceInfo = HomeRunDistance.ExtractDistanceInfoFromFrame(settings.Language ?? Language.ENGLISH, latestFrame);
                    if (distanceInfo is null)
                    {
                        return null;
                    }

                    return new HomeRunContestStats()
                    {
                        DistanceInfo = distanceInfo,
                    };
                }
            default:
                return null;
        }
    }

    public Metadata? GetMetadata()
    {
        if (_metadata is not null)
        {
            return _metadata;
        }

        using SlpFile slpFile = SlpReader.OpenSlpFile(_input);
        _metadata = slpFile.GetMetadata();

        return _metadata;
    }

    public string? GetFilePath()
    {
        if (_input is SlpFileReadInput fileInput)
        {
            return fileInput.FilePath;
        }

        return null;
    }

    public IList<Placement> GetWinners()
    {
        // Read game end block directly
        using SlpFile slpFile = SlpReader.OpenSlpFile(_input);
        GameEnd? gameEnd = slpFile.GetGameEnd();
        Process((_, _, _) => _parser.GetSettings() is not null, slpFile);

        GameStart? settings = _parser.GetSettings();
        if (gameEnd is null || settings is null)
        {
            // Technically using the final post frame updates, it should be possible to compute winners for
            // replays without a gameEnd message. But I'll leave this here anyway

            return [];
        }

        // If we went to time, let's fetch the post frame updates to compute the winner
        List<PostFrameUpdate> finalPostFrameUpdates = [];
        if (gameEnd.GameEndMethod == GameEndMethod.TIME)
        {
            finalPostFrameUpdates = slpFile.ExtractFinalPostFrameUpdates();
        }

        return WinnerCalculator.GetWinners(gameEnd, settings, finalPostFrameUpdates);
    }

    private void Process(EventCallbackFunc? shouldStop = null, SlpFile? file = null)
    {
        shouldStop ??= static (_, _, _) => false;

        if (_parser.GetGameEnd() is not null)
        {
            return;
        }

        SlpFile slpFile = file ?? SlpReader.OpenSlpFile(_input);
        // Generate settings from iterating through file
        _readPosition = slpFile.IterateEvents((command, payload, buffer) =>
        {
            if (payload is null)
            {
                // If payload is null, keep iterating. The parser probably just doesn't know
                // about this command yet
                return false;
            }

            _parser.HandleCommand(command, payload);

            return shouldStop(command, payload);
        }, _readPosition);

        if (file is null)
        {
            slpFile.Dispose();
        }
    }

    private void OnParserSettings(object? sender, GameStart settings)
    {
        _statsComputer.Setup(settings);
    }

    private void OnParserFinalizedFrame(object? sender, FrameEntry frame)
    {
        _statsComputer.AddFrame(frame);
    }

    public void Dispose()
    {
        _parser.OnSettings -= OnParserSettings;
        _parser.OnFinalizedFrame -= OnParserFinalizedFrame;
    }
}
