using Slippi.NET.Common;
using Slippi.NET.Stats.Types;
using Slippi.NET.Stats.Utils;
using Slippi.NET.Types;

namespace Slippi.NET.Stats;

public class ComboComputerEvent : IEvent<ComboComputerEventArgs>
{
    public required string Event { get; init; }
    public required ComboComputerEventArgs Args { get; init; }
}

public record class ComboComputerEventArgs
{
    public required ComboType Combo { get; init; }
    public required GameStartType? Settings { get; init; }
}

public class ComboComputer : EventEmitter<ComboComputerEvent, ComboComputerEventArgs>, IStatComputer<IList<ComboType>>
{
    private readonly Dictionary<PlayerIndexedType, ComboState> _state = [];
    private IList<PlayerIndexedType> _playerPermutations = [];
    private IList<ComboType> _combos = [];
    private GameStartType? _settings = null;
    
    public void Setup(GameStartType settings)
    {
        _settings = settings;
        _state.Clear();
        _combos.Clear();
        _playerPermutations = StatsUtils.GetSinglesPlayerPermutationsFromSettings(settings);

        foreach (var indices in _playerPermutations)
        {
            _state[indices] = new ComboState()
            {
                Combo = null,
                Move = null,
                ResetCounter = 0,
                LastHitAnimation = null,
                Event = null,
            };
        }
    }

    public void ProcessFrame(FrameEntryType newFrame, FramesType allFrames)
    {
        foreach (var indices in _playerPermutations)
        {
            if (_state.TryGetValue(indices, out ComboState? state) && state is not null)
            {
                HandleComboCompute(allFrames, state, indices, newFrame, _combos);

                if (state.Event is not null)
                {
                    Emit(new ComboComputerEvent() { Event = state.Event, Args = new ComboComputerEventArgs() { Combo = _combos[^1], Settings = _settings } });

                    state.Event = null;
                }
            }
        }
    }

    public IList<ComboType> Fetch()
    {
        return _combos;
    }

    private static void HandleComboCompute(
        FramesType frames,
        ComboState state,
        PlayerIndexedType indices,
        FrameEntryType frame,
        IList<ComboType> combos)
    {
        int currentFrameNumber = frame.Frame;
        PostFrameUpdateType playerFrame = frame.Players[indices.PlayerIndex]!.Post;
        PostFrameUpdateType opponentFrame = frame.Players[indices.OpponentIndex]!.Post;

        int prevFrameNumber = currentFrameNumber - 1;
        PostFrameUpdateType? prevPlayerFrame = null;
        PostFrameUpdateType? prevOpponentFrame = null;

        if (frames.TryGetValue(prevFrameNumber, out FrameEntryType? prevFrame))
        {
            prevPlayerFrame = prevFrame.Players[indices.PlayerIndex]!.Post;
            prevOpponentFrame = prevFrame.Players[indices.OpponentIndex]!.Post;
        }

        int oppActionStateId = opponentFrame.ActionStateId!.Value;
        bool opntIsDamaged = StatsUtils.IsDamaged((State)oppActionStateId);
        bool opntIsGrabbed = StatsUtils.IsGrabbed((State)oppActionStateId);
        bool opntIsCommandGrabbed = StatsUtils.IsCommandGrabbed((State)oppActionStateId);
        float opntDamageTaken = prevOpponentFrame is not null ? StatsUtils.CalcDamageTaken(opponentFrame, prevOpponentFrame) : 0;

        // Keep track of whether actionState changes after a hit. Used to compute move count
        // When purely using action state there was a bug where if you did two of the same
        // move really fast (such as ganon's jab), it would count as one move. Added
        // the actionStateCounter at this point which counts the number of frames since
        // an animation started. Should be more robust, for old files it should always be
        // null and null < null = false
        bool actionChangedSinceHit = playerFrame.ActionStateId != state.LastHitAnimation;
        int? actionCounter = playerFrame.ActionStateCounter;
        int? prevActionCounter = prevPlayerFrame is not null ? prevPlayerFrame.ActionStateCounter : 0;
        bool actionFrameCounterReset = actionCounter is not null && prevActionCounter is not null && actionCounter.Value < prevActionCounter.Value;
        if (actionChangedSinceHit || actionFrameCounterReset)
        {
            state.LastHitAnimation = null;
        }

        // If opponent took damage and was put in some kind of stun this frame, either
        // start a combo or count the moves for the existing combo
        if (opntIsDamaged || opntIsGrabbed || opntIsCommandGrabbed)
        {
            bool comboStarted = false;
            if (state.Combo is null)
            {
                state.Combo = new ComboType()
                {
                    PlayerIndex = indices.OpponentIndex,
                    StartFrame = currentFrameNumber,
                    EndFrame = null,
                    StartPercent = prevOpponentFrame is not null ? (prevOpponentFrame.Percent ?? 0) : 0,
                    CurrentPercent = opponentFrame.Percent ?? 0,
                    EndPercent = null,
                    Moves = [],
                    DidKill = false,
                    LastHitBy = indices.PlayerIndex,
                };

                combos.Add(state.Combo);

                // Track whether this is a new combo or not
                comboStarted = true;
            }

            if (opntDamageTaken > 0)
            {
                // If animation of last hit has been cleared that means this is a new move. This
                // prevents counting multiple hits from the same move such as fox's drill
                if (state.LastHitAnimation is null)
                {
                    state.Move = new MoveLandedType()
                    {
                        PlayerIndex = indices.PlayerIndex,
                        Frame = currentFrameNumber,
                        MoveId = playerFrame.LastAttackLanded!.Value,
                        HitCount = 0,
                        Damage = 0,
                    };

                    state.Combo.Moves.Add(state.Move);

                    // Make sure we don't overwrite the START event
                    if (!comboStarted)
                    {
                        state.Event = ComboEvent.COMBO_EXTEND;
                    }
                }

                if (state.Move is not null)
                {
                    state.Move.HitCount++;
                    state.Move.Damage += opntDamageTaken;
                }

                // Store previous frame animation to consider the case of a trade, the previous
                // frame should always be the move that actually connected... I hope
                state.LastHitAnimation = prevPlayerFrame is not null ? prevPlayerFrame.ActionStateId : null;
            }

            if (comboStarted)
            {
                state.Event = ComboEvent.COMBO_START;
            }
        }

        if (state.Combo is null)
        {
            // The rest of the function handles combo termination logic, so if we don't
            // have a combo started, there is no need to continue
            return;
        }

        bool opntIsTeching = StatsUtils.IsTeching((State)oppActionStateId);
        bool opntIsDowned = StatsUtils.IsDown((State)oppActionStateId);
        bool opntDidLoseStock = prevOpponentFrame is not null && StatsUtils.DidLoseStock(opponentFrame, prevOpponentFrame);
        bool opntIsDying = StatsUtils.IsDead((State)oppActionStateId);

        // Update precent if opponent didn't lose stock
        if (!opntDidLoseStock)
        {
            state.Combo.CurrentPercent = opponentFrame.Percent ?? 0;
        }

        if (opntIsDamaged || opntIsGrabbed || opntIsCommandGrabbed || opntIsTeching || opntIsDowned || opntIsDying)
        {
            // If opponent got grabbed or damaged, reset the reset counter
            state.ResetCounter = 0;
        }
        else
        {
            state.ResetCounter++;
        }

        bool shouldTerminate = false;

        // Termination condition 1 - player kills opponent
        if (opntDidLoseStock)
        {
            state.Combo.DidKill = true;
            shouldTerminate = true;
        }

        // Termination condition 2 - combo resets on time
        if (state.ResetCounter > Timers.COMBO_STRING_RESET_FRAMES)
        {
            shouldTerminate = true;
        }

        // If combo should terminate, mark the end states and add it to list
        if (shouldTerminate)
        {
            state.Combo.EndFrame = playerFrame.Frame;
            state.Combo.EndPercent = prevOpponentFrame is not null ? (prevOpponentFrame.Percent ?? 0) : 0;
            state.Event = ComboEvent.COMBO_END;

            state.Combo = null;
            state.Move = null;
        }
    }
}
