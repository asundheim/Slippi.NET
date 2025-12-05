using DolphinTestApp;
using Slippi.NET;

namespace DolphinConnectionTestApp;

public class Program
{
    public static void Main(string[] args)
    {
        if (args.Length == 2 && args[0] == "replay")
        {
            SlippiGame game = new SlippiGame(args[1]);
            game.GetMetadata();
            using DolphinLaunchTestApp testApp = new DolphinLaunchTestApp(args[1]);
            testApp.LaunchAndWait();
        }
        else
        {
            DolphinConnectionTestApp testApp = new DolphinConnectionTestApp();
            testApp.ConnectAndWait();
        }
    }
}
