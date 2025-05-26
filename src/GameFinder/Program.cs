using Slippi.NET;
using Slippi.NET.Melee;
using Slippi.NET.Melee.Types;
using Slippi.NET.Types;

namespace GameFinder;

internal class Program
{
    static void Main(string[] args)
    {
        string subfolder = args[0];
        Character? character = null;
        Console.WriteLine("Character: ");
        string? sCharacter = Console.ReadLine();
        if (!string.IsNullOrEmpty(sCharacter))
        {
            character = Enum.Parse<Character>(sCharacter, true);
        }

        Console.WriteLine("Color: ");
        string? color = Console.ReadLine();

        Stage? stage = null;
        Console.WriteLine("Stage: ");
        string? sStage = Console.ReadLine();
        Stage[] stages = [Stage.Dreamland, Stage.FinalDestination, Stage.PokemonStadium, Stage.Battlefield, Stage.YoshisStory, Stage.FountainOfDreams];
        if (!string.IsNullOrEmpty(sStage))
        {
            foreach (Stage eachStage in stages)
            {
                if (eachStage.ToString().StartsWith(sStage, StringComparison.OrdinalIgnoreCase))
                {
                    stage = eachStage;

                    break;
                }
            }
        }

        string fullPath = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Slippi", subfolder);
        foreach (var slpFilePath in Directory.EnumerateFiles(fullPath))
        {
            using SlippiGame game = new SlippiGame(slpFilePath);

            GameStart? settings = game.GetSettings();
            
            if (settings is not null)
            {
                if (character is not null)
                {
                    if (!settings.Players.Any(p => p.Character == character.Value &&
                            (color is not null ? color.Equals(CharacterUtils.GetCharacterColorName(p.Character!.Value, p.CharacterColor!.Value), StringComparison.OrdinalIgnoreCase) : true)))
                    {
                        continue;
                    }
                }

                if (stage is not null)
                {
                    if (settings.Stage != stage)
                    {
                        continue;
                    }
                }
            }
            else
            {
                continue;
            }

            Metadata? metadata = game.GetMetadata();
            Console.WriteLine("-------------------------------------------");
            Console.WriteLine(slpFilePath);
            if (metadata is not null)
            {
                Console.WriteLine(DateTime.Parse(metadata.StartAt!).ToLocalTime().ToString());
            }
            
            Player opp = settings.Players.Where(p => p.Character != Character.Fox).FirstOrDefault() ?? settings.Players.Where(p => p.DisplayName != "ders").First();
            Console.WriteLine($"{opp.DisplayName} {opp.ConnectCode}");
            Console.WriteLine("-------------------------------------------");
        }
    }
}
