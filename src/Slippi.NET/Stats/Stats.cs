using Slippi.NET.Types;

namespace Slippi.NET.Stats;

public class Stats
{
    private readonly StatOptions _options;
    private int? _lastProcessedFrame = null;
    private FramesType _frames = new FramesType();
    private List<int> _players = new List<int>();
    private readonly List<IStatComputer<object>> _allComputers = new List<IStatComputer<object>>();

    public Stats(StatOptions? options = null)
    {
        _options = options ?? new StatOptions { ProcessOnTheFly = false };
    }

    public void Setup(GameStart settings)
    {
        _frames = new FramesType();
        _players = settings.Players.ConvertAll(v => v.PlayerIndex);

        foreach (var comp in _allComputers)
        {
            comp.Setup(settings);
        }
    }

    public void Register(params IStatComputer<object>[] computers)
    {
        _allComputers.AddRange(computers);
    }

    public void Process()
    {
        if (_players.Count == 0)
        {
            return;
        }

        int i = _lastProcessedFrame.HasValue ? _lastProcessedFrame.Value + 1 : (int)Frames.FIRST;
        while (_frames.ContainsKey(i))
        {
            var frame = _frames[i];
            if (!IsCompletedFrame(_players, frame))
            {
                return;
            }

            foreach (var comp in _allComputers)
            {
                comp.ProcessFrame(frame, _frames);
            }

            _lastProcessedFrame = i;
            i++;
        }
    }

    public void AddFrame(FrameEntry frame)
    {
        _frames[frame.Frame!.Value] = frame;

        if (_options.ProcessOnTheFly)
        {
            Process();
        }
    }

    private static bool IsCompletedFrame(List<int> players, FrameEntry? frame)
    {
        if (frame == null)
        {
            return false;
        }

        foreach (var player in players)
        {
            if (frame.Players is null || !frame.Players.TryGetValue(player, out PlayerFrameData? value) || value?.Post is null)
            {
                return false;
            }
        }

        return true;
    }
}
