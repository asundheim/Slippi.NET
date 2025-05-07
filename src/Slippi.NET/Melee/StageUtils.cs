using Slippi.NET.Melee.Data;
using Slippi.NET.Melee.Types;
using System.Collections.Generic;

namespace Slippi.NET.Melee;

public static class StageUtils
{
    public static readonly StageInfo UnknownStage = new StageInfo(-1, "Unknown Stage");

    /// <summary>
    /// Gets information about a stage based on its ID.
    /// </summary>
    /// <param name="stageId">The ID of the stage.</param>
    /// <returns>A <see cref="StageInfo"/> object containing the stage ID and name.</returns>
    public static StageInfo GetStageInfo(int stageId)
    {
        return Stages.Lookup.TryGetValue(stageId, out var stage) ? stage : UnknownStage;
    }

    /// <summary>
    /// Gets the name of a stage based on its ID.
    /// </summary>
    /// <param name="stageId">The ID of the stage.</param>
    /// <returns>The name of the stage.</returns>
    public static string GetStageName(int stageId)
    {
        return GetStageInfo(stageId).Name;
    }
}