[![NuGet version (Slippi.NET)](https://img.shields.io/nuget/v/Slippi.NET.svg?style=flat-square)](https://www.nuget.org/packages/Slippi.NET/)
# Slippi.NET
This project is a port of [slippi-js](https://github.com/project-slippi/slippi-js) to modern .NET while maintaining more or less the same API surface. 

Internally an attempt is made to use stack-allocated `System.Span<byte>` wherever possible, which significantly improves performance over GC-allocated `byte[]`.

# Download
This package is available on [nuget.org](https://www.nuget.org/) and can be referenced by simply adding a `<PackageReference>` to your project like:
```
<ItemGroup>
    <PackageReference Include="Slippi.NET" Version="0.7.0" />
</ItemGroup>
```

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
`ConsoleConnection` is implemented but currently untested.

Using `DolphinConnection` is very straightforward, create a new `DolphinConnection` and call `Connect` with an IP address and port. You can listen to the
events defined on the base `Connection` class for updates and information on the Dolphin instance.

If Dolphin is local, you can use `127.0.0.1`. The `Ports` enumeration provides common values for ports, with `Default` (`51441`) being the standard
one that Dolphin uses.

You can find an example in `src/DolphinConnectionTestApp/`.

## Development
Restore and build `Slippi.NET.sln` from the root of the project.

Currently all projects target .NET 9 to make use of the latest `Span` improvements to the standard library.

## Tests
The `Slippi.NET.Tests` contains all unit tests for the project. All tests have been ported from `slippi-js`.
