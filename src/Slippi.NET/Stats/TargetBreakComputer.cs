using Slippi.NET.Types;
using Slippi.NET.Stats.Types;

namespace Slippi.NET.Stats;

public record class TargetBreakComputer : IStatComputer<IList<TargetBreakType>>
{
    private const int TARGET_ITEM_TYPE_ID = 209;

    private List<TargetBreakType> TargetBreaks { get; set; } = new();
    private bool IsTargetTestGame { get; set; } = false;

    public void Setup(GameStartType settings)
    {
        // Reset the state
        TargetBreaks.Clear();
        IsTargetTestGame = settings.GameMode == GameMode.TARGET_TEST;
    }

    public void ProcessFrame(FrameEntryType frame, FramesType allFrames)
    {
        if (!IsTargetTestGame)
        {
            return;
        }

        HandleTargetBreak(allFrames, frame, TargetBreaks);
    }

    public IList<TargetBreakType> Fetch()
    {
        return TargetBreaks;
    }

    private static void HandleTargetBreak(
        FramesType frames,
        FrameEntryType frame,
        List<TargetBreakType> targetBreaks)
    {
        var currentFrameNumber = frame.Frame;
        var prevFrameNumber = currentFrameNumber - 1;

        // Add all targets on the first frame
        if (currentFrameNumber == (int)Frames.FIRST)
        {
            var targets = frames[(int)Frames.FIRST]?.Items?
                .Where(item => item.TypeId == TARGET_ITEM_TYPE_ID)
                .ToList() ?? [];

            foreach (var target in targets)
            {
                targetBreaks.Add(new TargetBreakType
                {
                    SpawnId = target.SpawnId ?? 0,
                    FrameDestroyed = null,
                    PositionX = target.PositionX ?? 0,
                    PositionY = target.PositionY ?? 0
                });
            }
        }

        // Linq have mercy
        var currentTargets = frames[currentFrameNumber]?.Items?
            .Where(item => item.TypeId == TARGET_ITEM_TYPE_ID)
            .ToList() ?? [];

        var previousTargets = frames[prevFrameNumber]?.Items?
            .Where(item => item.TypeId == TARGET_ITEM_TYPE_ID)
            .ToList() ?? [];

        var currentTargetIds = currentTargets
            .Select(item => item.SpawnId)
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .ToList();

        var previousTargetIds = previousTargets
            .Select(item => item.SpawnId)
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .ToList();

        // Check if any targets were destroyed
        var brokenTargetIds = previousTargetIds
            .Where(id => !currentTargetIds.Contains(id))
            .ToList();

        foreach (var id in brokenTargetIds)
        {
            // Update the target break
            var targetBreak = targetBreaks.FirstOrDefault(tb => tb.SpawnId == id);
            if (targetBreak != null)
            {
                targetBreak.FrameDestroyed = currentFrameNumber;
            }
        }
    }
}