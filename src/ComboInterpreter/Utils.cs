using Slippi.NET.Stats.Types;
using Slippi.NET.Stats.Utils;
using Slippi.NET.Types;
using System.Text;

namespace ComboInterpreter;
internal static class Utils
{
    public static SimpleButtons GetTriggerButton(SimpleButtons allButtons) => allButtons.HasFlag(SimpleButtons.LT) ? SimpleButtons.LT : SimpleButtons.RT;

    public static SimpleButtons FacingDirectionToStick(bool facingLeft) => facingLeft ? SimpleButtons.STICK_LEFT : SimpleButtons.STICK_RIGHT;
    public static SimpleButtons FacingDirectionToOppositeStick(bool facingLeft) => facingLeft ? SimpleButtons.STICK_RIGHT : SimpleButtons.STICK_LEFT;
    public static SimpleButtons FacingDirectionToOppositeCstick(bool facingLeft) => facingLeft ? SimpleButtons.CSTICK_RIGHT : SimpleButtons.CSTICK_LEFT;
    public static SimpleButtons FacingDirectionToCstick(bool facingLeft) => facingLeft ? SimpleButtons.CSTICK_LEFT : SimpleButtons.CSTICK_RIGHT;
    public static SimpleButtons GetJumpButton(SimpleButtons allButtons) => allButtons.HasFlag(SimpleButtons.X) ? SimpleButtons.X : SimpleButtons.Y;

    public static SimpleButtons GetStick(SimpleButtons buttons)
    {
        if (buttons.HasFlag(SimpleButtons.STICK_UP))
        {
            if (buttons.HasFlag(SimpleButtons.STICK_RIGHT))
            {
                return SimpleButtons.STICK_UP | SimpleButtons.STICK_RIGHT;
            }
            else if (buttons.HasFlag(SimpleButtons.STICK_LEFT))
            {
                return SimpleButtons.STICK_UP | SimpleButtons.STICK_LEFT;
            }
            else
            {
                return SimpleButtons.STICK_UP;
            }
        }
        else if (buttons.HasFlag(SimpleButtons.STICK_DOWN))
        {
            if (buttons.HasFlag(SimpleButtons.STICK_RIGHT))
            {
                return SimpleButtons.STICK_DOWN | SimpleButtons.STICK_RIGHT;
            }
            else if (buttons.HasFlag(SimpleButtons.STICK_LEFT))
            {
                return SimpleButtons.STICK_DOWN | SimpleButtons.STICK_LEFT;
            }
            else
            {
                return SimpleButtons.STICK_DOWN;
            }
        }
        else if (buttons.HasFlag(SimpleButtons.STICK_LEFT))
        {
            return SimpleButtons.STICK_LEFT;
        }
        else if (buttons.HasFlag(SimpleButtons.STICK_RIGHT))
        {
            return SimpleButtons.STICK_RIGHT;
        }

        return SimpleButtons.NONE;
    }

    public static SimpleButtons ToSimpleButtons(this PreFrameUpdate frame)
    {
        SimpleButtons ret = SimpleButtons.NONE;
        ProcessedButtons[] allButtons = Enum.GetValues<ProcessedButtons>();
        for (int i = 0; i < allButtons.Length; i++)
        {
            var buttonToCheck = allButtons[i];
            if (frame.Buttons!.Value.HasFlag(buttonToCheck))
            {
                ret |= buttonToCheck switch
                {
                    ProcessedButtons.A => SimpleButtons.A,
                    ProcessedButtons.B => SimpleButtons.B,
                    ProcessedButtons.Z => SimpleButtons.Z,
                    ProcessedButtons.Y => SimpleButtons.Y,
                    ProcessedButtons.X => SimpleButtons.X,
                    ProcessedButtons.CStick_Down => SimpleButtons.CSTICK_DOWN,
                    ProcessedButtons.CStick_Left => SimpleButtons.CSTICK_LEFT,
                    ProcessedButtons.CStick_Right => SimpleButtons.CSTICK_RIGHT,
                    ProcessedButtons.CStick_Up => SimpleButtons.CSTICK_UP,
                    ProcessedButtons.RT => SimpleButtons.RT,
                    //ProcessedButtons.LT => SimpleButtons.LT,
                    //ProcessedButtons.Stick_Down => SimpleButtons.STICK_DOWN,
                    //ProcessedButtons.Stick_Up => SimpleButtons.STICK_UP,
                    //ProcessedButtons.Stick_Left => SimpleButtons.STICK_LEFT,
                    //ProcessedButtons.Stick_Right => SimpleButtons.STICK_RIGHT,
                    _ => SimpleButtons.NONE
                };
            }
        }

        JoystickRegion region = frame.GetJoystickRegion();
        if (region != JoystickRegion.DZ)
        {
            ret |= region switch
            {
                JoystickRegion.N => SimpleButtons.STICK_UP,
                JoystickRegion.E => SimpleButtons.STICK_RIGHT,
                JoystickRegion.S => SimpleButtons.STICK_DOWN,
                JoystickRegion.W => SimpleButtons.STICK_LEFT,
                JoystickRegion.NE => SimpleButtons.STICK_UP | SimpleButtons.STICK_RIGHT,
                JoystickRegion.NW => SimpleButtons.STICK_UP | SimpleButtons.STICK_LEFT,
                JoystickRegion.SE => SimpleButtons.STICK_DOWN | SimpleButtons.STICK_RIGHT,
                JoystickRegion.SW => SimpleButtons.STICK_DOWN | SimpleButtons.STICK_LEFT,
                _ => SimpleButtons.NONE
            };
        }

        return ret;
    }

    public static string ToDisplayString(this SimpleButtons buttons)
    {
        StringBuilder sb = new StringBuilder();

        SimpleButtons[] allButtons = Enum.GetValues<SimpleButtons>();
        for (int i = 0; i < allButtons.Length; i++)
        {
            var buttonToCheck = allButtons[i];
            if (buttons.HasFlag(buttonToCheck))
            {
                sb.Append(
                    buttonToCheck switch
                    {
                        SimpleButtons.A => "A",
                        SimpleButtons.B => "B",
                        SimpleButtons.LT => "L",
                        SimpleButtons.RT => "R",
                        SimpleButtons.X => "X",
                        SimpleButtons.Y => "Y",
                        SimpleButtons.CSTICK_UP => "C Stick Up",
                        SimpleButtons.CSTICK_DOWN => "C Stick Down",
                        SimpleButtons.STICK_LEFT => "C Stick Left",
                        SimpleButtons.CSTICK_RIGHT => "C Stick Right",
                        _ => string.Empty
                    }
                );

                sb.Append(" + ");
            }
        }

        if (buttons.HasFlag(SimpleButtons.STICK_UP))
        {
            if (buttons.HasFlag(SimpleButtons.STICK_RIGHT))
            {
                sb.Append("Up Right + ");
            }
            else if (buttons.HasFlag(SimpleButtons.STICK_LEFT))
            {
                sb.Append("Up Left + ");
            }
            else
            {
                sb.Append("Up + ");
            }
        }
        else if (buttons.HasFlag(SimpleButtons.STICK_DOWN))
        {
            if (buttons.HasFlag(SimpleButtons.STICK_RIGHT))
            {
                sb.Append("Down Right + ");
            }
            else if (buttons.HasFlag(SimpleButtons.STICK_LEFT))
            {
                sb.Append("Down Left + ");
            }
            else
            {
                sb.Append("Down + ");
            }
        }
        else if (buttons.HasFlag(SimpleButtons.STICK_LEFT))
        {
            sb.Append("Left + ");
        }
        else if (buttons.HasFlag(SimpleButtons.STICK_RIGHT))
        {
            sb.Append("Right + ");
        }

        return sb.ToString().TrimEnd(' ', '+');
    }
}
