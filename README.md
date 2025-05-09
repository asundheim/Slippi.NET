# Slippi.NET
This project is a port of [slippi-js](https://github.com/project-slippi/slippi-js) to modern .NET while maintaining more or less the same API surface. 

Internally an attempt is made to use stack-allocated `System.Span<byte>` wherever possible, which significantly improves performance over GC-allocated `byte[]`.

## Usage
Just as in `slippi-js`, the main entrypoint is `SlippiGame`, taking either a filepath or a byte buffer.
```csharp
SlippiGame game = new SlippiGame('./game.slp');
// do things with game
```

## Utils
Utility functions are scattered, to name a few:

### Slippi.NET.Melee
```csharp
namespace Slippi.NET.Melee;

static class StageUtils
{
    static StageInfo GetStageInfo(int stageId);
    // etc.
}

static class MoveUtils
{
    static Move GetMoveInfo(int moveId);
    // etc.
}

static class CharacterUtils
{
    static CharacterInfo GetCharacterInfo(int characterId);
    static List<CharacterInfo> GetAllCharacters();
    // etc.
}
```

### Slippi.NET.Stats
```csharp
namespace Slipp.NET.Stats;

static class ActionUtils
{
    static bool IsMissGroundTech(State animation);
    // etc.
}

static class InputUtils
{
    static JoystickRegion GetJoystickRegion(float x, float y);
    // etc.
}
```

### Slippi.NET.Utils
```csharp
namespace Slippi.NET.Utils;

static class WinnerCalculator
{
    static IList<Placement> GetWinners(
        GameEnd gameEnd,
        GameStart settings,
        IList<PostFrameUpdate> finalPostFrameUpdates);
}
```

## Connection
`DolphinConnection` and `ConsoleConnection` are implemented but currently untested.

## Development
Restore and build `Slippi.NET.sln` from the root of the project.

Currently all projects target .NET 9 to make use of the latest `Span` improvements to the standard library.

## Tests
The `Slippi.NET.Tests` contains all unit tests for the project. All tests have been ported from `slippi-js` aside from the tests in `filewriter.spec.ts` as `SlpFileWriter` has not been converted yet
.