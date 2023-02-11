using Banks.Banking;
using Banks.Models;
using Spectre.Console;

namespace Banks.CLI;

public class Program
{
    private static Dictionary<string, Handler> handlers = new ();

    internal static Bank? LastBank { get; set; } = null;
    internal static Client? LastClient => (Clients.Capacity > 0) ? Clients.Last() : null;
    internal static Account? LastUsedAccount { get; set; } = null;
    internal static List<Client> Clients { get; } = new List<Client>();
    internal static BankSettings Default { get; set; } = new BankSettings(0.1M, 0.1M, 50000M, 50000M, DefaultDepositPercent);

    public static int Main()
    {
        handlers.Add("make", new MakeHandler());
        handlers.Add("bank", new BankHandler());
        handlers.Add("account", new AccountHandler());
        handlers.Add("date", new DateHandler());
        handlers.Add("client", new ClientHandler());
        while (true)
            Run();
    }

    private static void Run()
    {
        AnsiConsole.Clear();
        var options = handlers.Keys.ToList();
        options.Sort();
        options.Add("exit");
        var choice = AnsiConsole.Prompt(new SelectionPrompt<string>()
            .Title("Select operation to perform")
            .AddChoices(options));

        if (choice == "exit")
            Exit();

        var handler = handlers[choice];
        try
        {
            if (handler.Run())
                Exit();
        }
        catch (Exception err)
        {
            AnsiConsole.Markup($"[red]{err}[/]");
            Thread.Sleep(1000);
        }
    }

    private static void Exit()
    {
        AnsiConsole.Clear();
        AnsiConsole.Write(
            new FigletText("Banks")
                .Centered()
                .Color(Color.Red));
        Environment.Exit(0);
    }

    private static decimal DefaultDepositPercent(decimal value)
    {
        if (value <= 50000)
            return 0.03M;
        if (value <= 100000)
            return 0.04M;
        return 0.05M;
    }
}