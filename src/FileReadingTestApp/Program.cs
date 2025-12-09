using Slippi.NET;
using Slippi.NET.Types;

namespace FileReadingTestApp;

internal class Program
{
    public static void Main()
    {
        // Only consider files with this year in the path
        const string yearFilter = "2025";

        // Filter our search to players with these connect codes.
        const string code1 = "D#10";
        const string code2 = "D#345";

        const bool writeLogs = false;

        string slpFolder = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Slippi");

        // Compute the total number of times the given player has been KO'd
        int koCount = 0;
        int fileCount = 0;
        int start = Environment.TickCount;
        foreach (var file in Directory.EnumerateFiles(slpFolder, searchPattern: "*", new EnumerationOptions() { RecurseSubdirectories = true }))
        {
            if (!file.Contains(yearFilter) || Path.GetExtension(file) != ".slp")
            {
                continue;
            }

            using var game = new SlippiGame(file);
            if (game.GetMetadata() is Metadata metadata)
            {
                foreach ((int playerIndex, PlayerMetadata player) in metadata.Players)
                {
                    if (player?.Names?.Code == code1 || player?.Names?.Code == code2)
                    {
                        koCount += 4 - game.GetLatestFrame()?.Players?[playerIndex]?.Post?.StocksRemaining ?? 4;
                        break;
                    }
                }
            }

            fileCount++;
            if (writeLogs)
            {
                Console.Write("\r");
                Console.Write($"Files: {fileCount}  KO Count: {koCount}  Files / s: {Math.Round(fileCount / ((Environment.TickCount - start) / 1000.0), 3)}");
            }
        }

        if (writeLogs)
        {
            Console.WriteLine($"KO count: {koCount}");
        }
    }
}
