namespace DolphinConnectionTestApp;

public class Program
{
    public static void Main(string[] args)
    {
        DolphinConnectionTestApp testApp = new DolphinConnectionTestApp();
        testApp.ConnectAndWait();
    }
}
