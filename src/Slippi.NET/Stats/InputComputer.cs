using Slippi.NET.Types;
using Slippi.NET.Stats.Utils;
using Slippi.NET.Stats.Types;
using static Slippi.NET.Stats.Utils.InputUtils;

namespace Slippi.NET.Stats;

public record class InputComputer : IStatComputer<IList<PlayerInput>>
{
    private readonly Dictionary<PlayerIndexedType, PlayerInput> _state = new();
    private List<PlayerIndexedType> _playerPermutations = new();

    public void Setup(GameStart settings)
    {
        // Reset the state
        _state.Clear();
        _playerPermutations = StatsUtils.GetSinglesPlayerPermutationsFromSettings(settings).ToList();

        foreach (var indices in _playerPermutations)
        {
            var playerState = new PlayerInput()
            {
                PlayerIndex = indices.PlayerIndex,
                OpponentIndex = indices.OpponentIndex,
                InputCount = 0,
                JoystickInputCount = 0,
                CstickInputCount = 0,
                ButtonInputCount = 0,
                TriggerInputCount = 0,
            };

            _state[indices] = playerState;
        }
    }

    public void ProcessFrame(FrameEntry frame, FramesType allFrames)
    {
        foreach (var indices in _playerPermutations)
        {
            if (_state.TryGetValue(indices, out var state))
            {
                HandleInputCompute(allFrames, state, indices, frame);
            }
        }
    }

    public IList<PlayerInput> Fetch()
    {
        return _state.Values.ToList();
    }

    private static void HandleInputCompute(
        FramesType frames,
        PlayerInput state,
        PlayerIndexedType indices,
        FrameEntry frame)
    {
        var playerFrame = frame.Players![indices.PlayerIndex]?.Pre;
        if (playerFrame == null) return;

        var currentFrameNumber = playerFrame.Frame ?? 0;
        var prevFrameNumber = currentFrameNumber - 1;
        var prevPlayerFrame = frames.TryGetValue(prevFrameNumber, out var prevFrame)
            ? prevFrame.Players![indices.PlayerIndex]?.Pre
            : null;

        if (currentFrameNumber < (int)Frames.FIRST_PLAYABLE || prevPlayerFrame == null)
        {
            // Don't count inputs until the game actually starts
            return;
        }

        // Count button presses
        var invertedPreviousButtons = ~prevPlayerFrame.PhysicalButtons ?? 0;
        var currentButtons = playerFrame.PhysicalButtons ?? 0;
        var buttonChanges = invertedPreviousButtons & currentButtons & 0xfff;
        var newInputsPressed = CountSetBits(buttonChanges);
        state.InputCount += newInputsPressed;
        state.ButtonInputCount += newInputsPressed;

        // Count joystick region changes
        var prevAnalogRegion = GetJoystickRegion(prevPlayerFrame.JoystickX ?? 0, prevPlayerFrame.JoystickY ?? 0);
        var currentAnalogRegion = GetJoystickRegion(playerFrame.JoystickX ?? 0, playerFrame.JoystickY ?? 0);
        if (prevAnalogRegion != currentAnalogRegion && currentAnalogRegion != JoystickRegion.DZ)
        {
            state.InputCount += 1;
            state.JoystickInputCount += 1;
        }

        // Count c-stick region changes
        var prevCstickRegion = GetJoystickRegion(prevPlayerFrame.CStickX ?? 0, prevPlayerFrame.CStickY ?? 0);
        var currentCstickRegion = GetJoystickRegion(playerFrame.CStickX ?? 0, playerFrame.CStickY ?? 0);
        if (prevCstickRegion != currentCstickRegion && currentCstickRegion != JoystickRegion.DZ)
        {
            state.InputCount += 1;
            state.CstickInputCount += 1;
        }

        // Count trigger presses
        if (prevPlayerFrame.PhysicalLTrigger < 0.3 && playerFrame.PhysicalLTrigger >= 0.3)
        {
            state.InputCount += 1;
            state.TriggerInputCount += 1;
        }
        if (prevPlayerFrame.PhysicalRTrigger < 0.3 && playerFrame.PhysicalRTrigger >= 0.3)
        {
            state.InputCount += 1;
            state.TriggerInputCount += 1;
        }
    }
}