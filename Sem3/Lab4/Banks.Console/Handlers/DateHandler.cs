using Banks.Banking;
using Spectre.Console;
namespace Banks.CLI;

public class DateHandler : Handler
{
    public override bool Run()
    {
        AnsiConsole.WriteLine($"Old date: {CentralBank.Shared.Time}");
        string strDate = AnsiConsole.Ask<string>("New date: ");
        DateTime date = DateTime.Parse(strDate);
        if (AnsiConsole.Confirm("Confirm changing date"))
        {
            try
            {
                CentralBank.Shared.ChangeTime(date);
            }
            catch (Exception err)
            {
                PrintError($"ERROR: {err}");
            }
        }

        return false;
    }
}