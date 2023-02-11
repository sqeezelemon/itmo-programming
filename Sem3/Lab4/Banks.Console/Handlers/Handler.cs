using Spectre.Console;
namespace Banks.CLI;

public abstract class Handler
{
    public abstract bool Run();

    protected bool PrintError(string message)
    {
        AnsiConsole.Markup($"[red]{message}[/]");
        Thread.Sleep(1000);
        return false;
    }

    protected bool PrintSuccess(string message)
    {
        AnsiConsole.Markup($"[green]{message}[/]");
        Thread.Sleep(1000);
        return false;
    }

    protected void Wait()
    {
        AnsiConsole.Prompt(
            new TextPrompt<string>("Press enter to exit")
                .AllowEmpty()
                .Secret());
    }
}