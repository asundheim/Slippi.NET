namespace Slippi.NET.Types;

public record class PreFrameUpdate
{
    public PreFrameUpdate(
        int? frame, 
        byte? playerIndex, 
        bool? isFollower, 
        uint? seed, 
        ActionState? actionStateId, 
        float? positionX, 
        float? positionY, 
        float? facingDirection, 
        float? joystickX, 
        float? joystickY, 
        float? cStickX, 
        float? cStickY, 
        float? trigger, 
        ProcessedButtons? buttons, 
        PhysicalButtons? physicalButtons, 
        float? physicalLTrigger, 
        float? physicalRTrigger, 
        sbyte? rawJoystickX, 
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

    public int? Frame;
    public byte? PlayerIndex;
    public bool? IsFollower;
    public ActionState? ActionStateId;
    public float? PositionX;
    public float? PositionY;
    public float? FacingDirection;
    public uint? Seed;
    public float? JoystickX;
    public float? JoystickY;
    public float? CStickX;
    public float? CStickY;
    public float? Trigger;
    public ProcessedButtons? Buttons;
    public PhysicalButtons? PhysicalButtons;
    public float? PhysicalLTrigger;
    public float? PhysicalRTrigger;
    public sbyte? RawJoystickX;
    public float? Percent;
}