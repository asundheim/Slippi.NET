using Slippi.NET.Types;
using static Slippi.NET.Types.HomeRunDistanceUnits;

namespace Slippi.NET.Utils;

public static class HomeRunDistance
{
    private const float FEET_CONVERSION_FACTOR = 0.952462f;
    private const float METERS_CONVERSION_FACTOR = 1.04167f;

    private const int SANDBAG_INTERNAL_ID = 32;

    public static float PositionToHomeRunDistance(float distance, string units)
    {
        float score = 0;
        switch (units) 
        {
            case FEET:
                {
                    score = 10 * MathF.Floor(distance - (70 * FEET_CONVERSION_FACTOR));
                    score = MathF.Round(score);
                    score = MathF.Floor((score / 30.4788f) * 10) / 10;

                    break;
                }
            case METERS:
                {
                    score = 10 * MathF.Floor(distance - (70 * METERS_CONVERSION_FACTOR));
                    score = MathF.Round(score);
                    score = MathF.Floor((score / 100) * 10) / 10;

                    break;
                }
            default:
                {
                    System.Console.WriteLine($"Unknown units: {units}");
                    throw new ArgumentException($"Unknown units: {units}", nameof(units));
                }
                
        }

        score = MathF.Round(score * 10) / 10;

        return MathF.Max(0f, score);
    }

    public static HomeRunDistanceInfo? ExtractDistanceInfoFromFrame(Language language, FrameEntryType lastFrame)
    {
        var sandbagLastFrame = lastFrame.Players.Values
            .FirstOrDefault(playerFrame => playerFrame is not null && playerFrame.Post.InternalCharacterId == SANDBAG_INTERNAL_ID);

        if (sandbagLastFrame is null)
        {
            return null;
        }

        // Only return the distance in meters if it's a Japanese replay.
        // Technically we should check if the replay is PAL but we don't yet support
        // stadium replays in PAL.
        string units = language == Language.JAPANESE ? METERS : FEET;
        float distance = PositionToHomeRunDistance(sandbagLastFrame.Post.PositionX ?? 0, units);

        return new HomeRunDistanceInfo()
        {
            Distance = distance,
            Units = units,
        };
    }
}
