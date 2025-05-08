
using Slippi.NET.Types;

namespace Slippi.NET.Utils;

public class RollbackCounter
{
    private readonly RollbackFramesCollection _rollbackFrames = [];
    private int _rollbackFrameCount = 0;
    private int? _rollbackPlayerIdx = null; // For keeping track of rollbacks by following a single player
    private bool _lastFrameWasRollback = false;
    private int _currentRollbackLength = 0;
    private readonly List<int> _rollbackLengths = [];

    /// <summary>
    /// Checks if the current frame is a rollback frame for a specific player.
    /// </summary>
    /// <param name="currentFrame">The current frame entry.</param>
    /// <param name="playerIdx">The index of the player to track.</param>
    /// <returns>True if the last frame was a rollback, otherwise false.</returns>
    public bool CheckIfRollbackFrame(FrameEntry? currentFrame, int playerIdx)
    {
        if (_rollbackPlayerIdx == null)
        {
            // We only want to follow a single player to avoid double counting. So we use whoever is first.
            _rollbackPlayerIdx = playerIdx;
        }
        else if (_rollbackPlayerIdx != playerIdx)
        {
            return false;
        }

        if (currentFrame != null && currentFrame.Players != null)
        {
            // Frame already exists for currentFrameNumber, so we must be rolling back.
            // Note: We detect during PreFrameUpdate, but new versions have a
            // FrameStart command that has already initialized the frame, so we must
            // check for player data too.
            if (_rollbackFrames.TryGetValue(currentFrame.Frame, out List<FrameEntry>? value))
            {
                value.Add(currentFrame);
            }
            else
            {
                _rollbackFrames[currentFrame.Frame] = [currentFrame];
            }

            _rollbackFrameCount++;
            _currentRollbackLength++;
            _lastFrameWasRollback = true;
        }
        else if (_lastFrameWasRollback)
        {
            _rollbackLengths.Add(_currentRollbackLength);
            _currentRollbackLength = 0;
            _lastFrameWasRollback = false;
        }

        return _lastFrameWasRollback;
    }

    /// <summary>
    /// Gets the rollback frames.
    /// </summary>
    /// <returns>A dictionary of rollback frames.</returns>
    public RollbackFramesCollection GetFrames()
    {
        return _rollbackFrames;
    }

    /// <summary>
    /// Gets the total count of rollback frames.
    /// </summary>
    /// <returns>The total rollback frame count.</returns>
    public int GetCount()
    {
        return _rollbackFrameCount;
    }

    /// <summary>
    /// Gets the lengths of rollbacks.
    /// </summary>
    /// <returns>A list of rollback lengths.</returns>
    public List<int> GetLengths()
    {
        return _rollbackLengths;
    }
}
