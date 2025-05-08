namespace Slippi.NET.Types;

public record class PreFrameUpdate : FrameUpdate
{
    public PreFrameUpdate(
        int? frame, 
        byte? playerIndex, 
        bool? isFollower, 
        uint? seed, 
        ushort? actionStateId, 
        float? positionX, 
        float? positionY, 
        float? facingDirection, 
        float? joystickX, 
        float? joystickY, 
        float? cStickX, 
        float? cStickY, 
        float? trigger, 
        uint? buttons, 
        ushort? physicalButtons, 
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

    public uint? Seed { get; set; }
    public float? JoystickX { get; set; }
    public float? JoystickY { get; set; }
    public float? CStickX { get; set; }
    public float? CStickY { get; set; }
    public float? Trigger { get; set; }
    public uint? Buttons { get; set; }
    public ushort? PhysicalButtons { get; set; }
    public float? PhysicalLTrigger { get; set; }
    public float? PhysicalRTrigger { get; set; }
    public sbyte? RawJoystickX { get; set; }
    public float? Percent { get; set; }
}