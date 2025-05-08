using Slippi.NET.Common;
using Slippi.NET.Stats.Types;
using Slippi.NET.Stats.Utils;
using Slippi.NET.Types;

namespace Slippi.NET.Stats;

public class ConversionsComputerEvent : IEvent<ConversionsComputerEventArgs>
{
    public string Event => "CONVERSION";

    public required ConversionsComputerEventArgs Args { get; init; }
}

public class ConversionsComputerEventArgs
{
    public required ConversionType? Combo { get; init; }
    public required GameStart? Settings { get; init; }
}

public class ConversionsComputer : EventEmitter<ConversionsComputerEvent, ConversionsComputerEventArgs>, IStatComputer<IList<ConversionType>>
{
    private readonly Dictionary<PlayerIndexedType, PlayerConversionState> _state = [];
    private IList<PlayerIndexedType> _playerPermutations = [];
    private IList<ConversionType> _conversions = [];
    private LastEndFrameMetadataType _metadata;
    private GameStart? _settings = null;

    public ConversionsComputer()
    {
        _metadata = new LastEndFrameMetadataType();
    }

    public void Setup(GameStart settings)
    {
        _playerPermutations = StatsUtils.GetSinglesPlayerPermutationsFromSettings(settings);
        _conversions.Clear();
        _state.Clear();
        _metadata = new LastEndFrameMetadataType();
        _settings = settings;

        foreach (var indices in _playerPermutations)
        {
            _state[indices] = new PlayerConversionState()
            {
                Conversion = null,
                Move = null,
                ResetCounter = 0,
                LastHitAnimation = null,
            };
        }
    }

    public void ProcessFrame(FrameEntry newFrame, FramesType allFrames)
    {
        foreach (var indices in _playerPermutations)
        {
            if (_state.TryGetValue(indices, out PlayerConversionState? state))
            {
                bool terminated = HandleConversionCompute(allFrames, state, indices, newFrame, _conversions);
                if (terminated)
                {
                    Emit(new ConversionsComputerEvent()
                    {
                        Args = new ConversionsComputerEventArgs()
                        {
                            Combo = _conversions.Count > 0 ? _conversions[^1] : null,
                            Settings = _settings
                        }
                    });
                }
            }
        }
    }

    public IList<ConversionType> Fetch()
    {
        PopulateConversionTypes();

        return _conversions;
    }

    private void PopulateConversionTypes()
    {
        // Post-processing step: set the openingTypes
        var conversionsToHandle = _conversions.Where(c => c.OpeningType != "unknown").ToList();

        // Group new conversions by startTime and sort
        var groupedConversions = _conversions.GroupBy(c => c.StartFrame);
        var sortedConversion = groupedConversions.OrderBy(c => c.FirstOrDefault()?.StartFrame ?? 0);

        // Set the opening types on the conversions we need to handle
        foreach (var conversions in sortedConversion.Select(c => c.ToList()))
        {
            bool isTrade = conversions.Count >= 2;
            foreach (var conversion in conversions)
            {
                // Set end frame for this conversion
                _metadata.LastEndFrameByOppIdx[conversion.PlayerIndex] = conversion.EndFrame!.Value;

                if (isTrade)
                {
                    conversion.OpeningType = "trade";
                    break;
                }

                // If not trade, check the opponent endFrame
                MoveLandedType? lastMove = conversion.Moves.Count > 0 ? conversion.Moves[^1] : null;
                int? oppEndFrame = _metadata.LastEndFrameByOppIdx.TryGetValue(lastMove is not null ? lastMove.PlayerIndex : conversion.PlayerIndex, out int endFrame) ? endFrame : null;
                bool isCounterAttack = oppEndFrame is not null && oppEndFrame.Value > conversion.StartFrame;
                conversion.OpeningType = isCounterAttack ? "counter-attack" : "neutral-win";
            }
        }
    }

    private static bool HandleConversionCompute(
        FramesType frames,
        PlayerConversionState state,
        PlayerIndexedType indices,
        FrameEntry frame,
        IList<ConversionType> conversions)
    {
        int currentFrameNumber = frame.Frame!.Value;
        var playerFrame = frame.Players![indices.PlayerIndex]!.Post;
        var opponentFrame = frame.Players[indices.OpponentIndex]!.Post;

        int prevFrameNumber = currentFrameNumber - 1;
        PostFrameUpdate? prevPlayerFrame = null;
        PostFrameUpdate? prevOpponentFrame = null;

        if (frames.TryGetValue(prevFrameNumber, out var prevFrame))
        {
            prevPlayerFrame = prevFrame.Players![indices.PlayerIndex]!.Post;
            prevOpponentFrame = prevFrame.Players[indices.OpponentIndex]!.Post;
        }

        var oppActionStateId = opponentFrame!.ActionStateId;
        bool opntIsDamaged = oppActionStateId is not null && StatsUtils.IsDamaged((State)oppActionStateId);
        bool opntIsGrabbed = oppActionStateId is not null && StatsUtils.IsGrabbed((State)oppActionStateId);
        bool opntIsCommandGrabbed = oppActionStateId is not null && StatsUtils.IsCommandGrabbed((State)oppActionStateId);
        float opntDamageTaken = prevOpponentFrame is not null ? StatsUtils.CalcDamageTaken(opponentFrame, prevOpponentFrame) : 0;

        // Keep track of whether actionState changes after a hit. Used to compute move count
        // When purely using action state there was a bug where if you did two of the same
        // move really fast (such as ganon's jab), it would count as one move. Added
        // the actionStateCounter at this point which counts the number of frames since
        // an animation started. Should be more robust, for old files it should always be
        // null and null < null = false
        bool actionChangedSinceHit = playerFrame!.ActionStateId != state.LastHitAnimation;
        float? actionCounter = playerFrame.ActionStateCounter;
        float prevActionCounter = prevPlayerFrame?.ActionStateCounter ?? 0;
        bool actionFrameCounterReset = actionCounter < prevActionCounter;

        if (actionChangedSinceHit || actionFrameCounterReset)
        {
            state.LastHitAnimation = null;
        }

        // If opponent took damage and was put in some kind of stun this frame, either
        // start a conversion or
        if (opntIsDamaged || opntIsGrabbed || opntIsCommandGrabbed)
        {
            if (state.Conversion is null)
            {
                state.Conversion = new ConversionType
                {
                    PlayerIndex = indices.OpponentIndex,
                    LastHitBy = indices.PlayerIndex,
                    StartFrame = currentFrameNumber,
                    EndFrame = null,
                    StartPercent = prevOpponentFrame?.Percent ?? 0,
                    CurrentPercent = opponentFrame.Percent ?? 0,
                    EndPercent = null,
                    Moves = new List<MoveLandedType>(),
                    DidKill = false,
                    OpeningType = "unknown" // Will be updated later (5 years ago) (lol)
                };

                conversions.Add(state.Conversion);
            }

            if (opntDamageTaken > 0)
            {
                // If animation of last hit has been cleared that means this is a new move. This
                // prevents counting multiple hits from the same move such as fox's drill
                if (state.LastHitAnimation is null)
                {
                    state.Move = new MoveLandedType
                    {
                        PlayerIndex = indices.PlayerIndex,
                        Frame = currentFrameNumber,
                        MoveId = playerFrame.LastAttackLanded ?? -1,
                        HitCount = 0,
                        Damage = 0
                    };

                    state.Conversion.Moves.Add(state.Move);
                }

                if (state.Move is not null)
                {
                    state.Move.HitCount += 1;
                    state.Move.Damage += opntDamageTaken;
                }

                // Store previous frame animation to consider the case of a trade, the previous
                // frame should always be the move that actually connected... I hope
                state.LastHitAnimation = prevPlayerFrame?.ActionStateId;
            }
        }

        if (state.Conversion is null)
        {
            // The rest of the function handles conversion termination logic, so if we don't
            // have a conversion started, there is no need to continue
            return false;
        }

        bool opntInControl = oppActionStateId is not null && StatsUtils.IsInControl((State)oppActionStateId);
        bool opntDidLoseStock = prevOpponentFrame is not null && StatsUtils.DidLoseStock(opponentFrame, prevOpponentFrame);

        // Update percent if opponent didn't lose stock
        if (!opntDidLoseStock)
        {
            state.Conversion.CurrentPercent = opponentFrame.Percent ?? 0;
        }

        if (opntIsDamaged || opntIsGrabbed || opntIsCommandGrabbed)
        {
            // If opponent got grabbed or damaged, reset the reset counter
            state.ResetCounter = 0;
        }

        bool shouldStartResetCounter = state.ResetCounter == 0 && opntInControl;
        bool shouldContinueResetCounter = state.ResetCounter > 0;

        if (shouldStartResetCounter || shouldContinueResetCounter)
        {
            // This will increment the reset timer under the following conditions:
            // 1) if we were punishing opponent but they have now entered an actionable state
            // 2) if counter has already started counting meaning opponent has entered actionable state
            state.ResetCounter += 1;
        }

        bool shouldTerminate = false;

        // Termination condition 1 - player kills opponent
        if (opntDidLoseStock)
        {
            state.Conversion.DidKill = true;
            shouldTerminate = true;
        }

        // Termination condition 2 - conversion resets on time
        if (state.ResetCounter > Timers.PUNISH_RESET_FRAMES)
        {
            shouldTerminate = true;
        }

        // If conversion should terminate, mark the end states and add it to list
        if (shouldTerminate)
        {
            state.Conversion.EndFrame = playerFrame.Frame;
            state.Conversion.EndPercent = prevOpponentFrame is not null ? (prevOpponentFrame.Percent ?? 0) : 0;

            state.Conversion = null;
            state.Move = null;
        }

        return shouldTerminate;
    }
}
