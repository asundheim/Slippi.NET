using Slippi.NET.Console.Types;
using Slippi.NET.Melee.Types;
using System.IO;

namespace Slippi.NET.Console.Utils;

public static class MenuUtils
{
    public static MenuScene GetMenuScene(ushort? sceneId) =>
        sceneId switch
        {
            0x00 => MenuScene.PRESS_START,
            0x01 => MenuScene.MAIN_MENU,
            0x02 => MenuScene.CHARACTER_SELECT,
            0x08 => MenuScene.SLIPPI_ONLINE_CSS,
            >= 0x102 and <= 0x0108 => MenuScene.STAGE_SELECT,
            0x0202 => MenuScene.IN_GAME,
            0x0402 => MenuScene.POSTGAME_SCORES,
            _ => MenuScene.UNKNOWN_MENU
        };

    public static Character? ConvertMenuCharacter(this MenuCharacter menuCharacter) =>
        menuCharacter switch
        {
            MenuCharacter.DrMario => Character.DrMario,
            MenuCharacter.Mario => Character.Mario,
            MenuCharacter.Luigi => Character.Luigi,
            MenuCharacter.Bowser => Character.Bowser,
            MenuCharacter.Peach => Character.Peach,
            MenuCharacter.Yoshi => Character.Yoshi,
            MenuCharacter.DonkeyKong => Character.DonkeyKong,
            MenuCharacter.CaptainFalcon => Character.CaptainFalcon,
            MenuCharacter.Ganondorf => Character.Ganondorf,
            MenuCharacter.Falco => Character.Falco,
            MenuCharacter.Fox => Character.Fox,
            MenuCharacter.Ness => Character.Ness,
            MenuCharacter.IceClimbers => Character.IceClimbers,
            MenuCharacter.Kirby => Character.Kirby,
            MenuCharacter.Samus => Character.Samus,
            MenuCharacter.Zelda => Character.Zelda,
            MenuCharacter.Link => Character.Link,
            MenuCharacter.YoungLink => Character.YoungLink,
            MenuCharacter.Pichu => Character.Pichu,
            MenuCharacter.Pikachu => Character.Pikachu,
            MenuCharacter.JigglyPuff => Character.JigglyPuff,
            MenuCharacter.Mewtwo => Character.Mewtwo,
            MenuCharacter.Marth => Character.Marth,
            MenuCharacter.Roy => Character.Roy,
            _ => null,
        };
}
