using ComboInterpreter;
using Slippi.NET.Stats.Types;
using Slippi.NET.Stats.Utils;
using Slippi.NET.Types;
using Slippi.NET.Utils;

namespace Slippi.NET.Stats;

/// <summary>
/// Emit events when DI is calculated
/// </summary>
public class DIComputer : IStatComputer<string>
{
    private IList<PlayerIndices> _playerPermutations = [];

    public event EventHandler<DIEventArgs>? OnDI;

    public void Setup(GameStart settings)
    {
        _playerPermutations = StatsUtils.GetSinglesPlayerPermutationsFromSettings(settings);
    }

    public void ProcessFrame(FrameEntry newFrame, Dictionary<int, FrameEntry> allFrames)
    {
        foreach (var indices in _playerPermutations)
        {
            CheckForDI(indices, newFrame, allFrames);
        }
    }

    private void CheckForDI(PlayerIndices indices, FrameEntry frame, Dictionary<int, FrameEntry> allFrames)
    {
        var playerPre = frame.Players![indices.PlayerIndex]!.Pre!;
        var playerPost = frame.Players![indices.PlayerIndex]!.Post!;
        var oppPost = frame.Players![indices.OpponentIndex]!.Post!;

        // hits
        if (playerPost.HitlagRemaining == 1 && (playerPost.StateFlags2?.HasFlag(StateFlags2.IsDefenderInHitLag) ?? false))
        {
            OnDI?.Invoke(this, new DIEventArgs()
            {
                PlayerIndex = indices.PlayerIndex,
                PreFrameUpdate = playerPre,
                PostFrameUpdate = playerPost,
            });
        }
        // throws
        else if (!playerPost.ActionStateId.IsThrown() && allFrames.TryGetValue(frame.Frame!.Value - 1, out FrameEntry? prevFrame))
        {
            var prevPlayerPost = prevFrame!.Players![indices.PlayerIndex]!.Post!;
            if (prevPlayerPost?.ActionStateId.IsThrown() == true)
            {
                OnDI?.Invoke(this, new DIEventArgs()
                {
                    PlayerIndex = indices.PlayerIndex,
                    PreFrameUpdate = playerPre,
                    PostFrameUpdate = playerPost,
                });
            }
        }

    }

    public string Fetch()
    {
        return string.Empty;
    }
}
