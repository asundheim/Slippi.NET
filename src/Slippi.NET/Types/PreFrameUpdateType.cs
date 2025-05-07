namespace Slippi.NET.Types;

public record class PreFrameUpdateType
{
    public PreFrameUpdateType(
        int? frame, 
        int? playerIndex, 
        bool? isFollower, 
        int? seed, 
        int? actionStateId, 
        float? positionX, 
        float? positionY, 
        float? facingDirection, 
        float? joystickX, 
        float? joystickY, 
        float? cStickX, 
        float? cStickY, 
        float? trigger, 
        int? buttons, 
        int? physicalButtons, 
        float? physicalLTrigger, 
        float? physicalRTrigger, 
        float? rawJoystickX, 
        float? percent)
    {
        Frame = frame;
        PlayerIndex = playerIndex;
        IsFollower = isFollower;
        Seed = seed;
        ActionStateId = actionStateId;
        PositionX = positionX;
        PositionY = positionY;
        FacingDirection = facingDirection;
        JoystickX = joystickX;
        JoystickY = joystickY;
        CStickX = cStickX;
        CStickY = cStickY;
        Trigger = trigger;
        Buttons = buttons;
        PhysicalButtons = physicalButtons;
        PhysicalLTrigger = physicalLTrigger;
        PhysicalRTrigger = physicalRTrigger;
        RawJoystickX = rawJoystickX;
        Percent = percent;
    }

    public int? Frame { get; set; }
    public int? PlayerIndex { get; set; }
    public bool? IsFollower { get; set; }
    public int? Seed { get; set; }
    public int? ActionStateId { get; set; }
    public float? PositionX { get; set; }
    public float? PositionY { get; set; }
    public float? FacingDirection { get; set; }
    public float? JoystickX { get; set; }
    public float? JoystickY { get; set; }
    public float? CStickX { get; set; }
    public float? CStickY { get; set; }
    public float? Trigger { get; set; }
    public int? Buttons { get; set; }
    public int? PhysicalButtons { get; set; }
    public float? PhysicalLTrigger { get; set; }
    public float? PhysicalRTrigger { get; set; }
    public float? RawJoystickX { get; set; }
    public float? Percent { get; set; }
}