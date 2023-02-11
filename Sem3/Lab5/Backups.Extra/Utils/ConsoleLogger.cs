namespace Backups.Extra.Utils;

public class ConsoleLogger : Logger
{
    protected override void Write(string message) => Console.WriteLine(message);
}