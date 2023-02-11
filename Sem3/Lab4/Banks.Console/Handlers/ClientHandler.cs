using Banks.Banking;
using Banks.Models;
using Spectre.Console;

namespace Banks.CLI;

public class ClientHandler : Handler
{
    public override bool Run()
    {
        if (CentralBank.Shared.Banks.Sum(b => b.Clients.Count) == 0)
        {
            PrintError("No clients found");
            return false;
        }

        var clientChoice = AnsiConsole.Prompt<string>(new SelectionPrompt<string>()
            .Title("Select client")
            .AddChoices(
                CentralBank.Shared.Banks
                .SelectMany(b => b.Clients)
                .GroupBy(c => c.PhoneNumber)
                .Select(g => g.First())
                .Select(c => $"{c.PhoneNumber} - {c.Name}")));
        Client client = CentralBank.Shared.Banks
            .SelectMany(b => b.Clients)
            .First(c => c.PhoneNumber == clientChoice.Split(" ")[0]);

        var choice = AnsiConsole.Prompt(new SelectionPrompt<string>()
            .Title("Select operation to perform")
            .AddChoices(new[]
            {
               "info", "phone", "passport", "address", "exit",
            }));
        switch (choice)
        {
            case "info":
                return Info(client);
            case "phone":
                return SetPhone(client);
            case "passport":
                return SetPassport(client);
            case "address":
                return SetAddress(client);
            default:
                return false;
        }
    }

    private bool Info(Client client)
    {
        var table = new Table();
        table.AddColumns(new string[]
        {
            "PROPERTY", "VALUE",
        });
        table.Border(TableBorder.Minimal);
        table.AddRow("Phone", client.PhoneNumber);
        table.AddRow("Name", client.Name);
        table.AddRow("Passport", client.PhoneNumber);
        table.AddRow("Address", client.Address == null ? string.Empty : client.Address.Value);
        AnsiConsole.Write(table);
        Wait();
        return false;
    }

    private bool SetPhone(Client client)
    {
        AnsiConsole.WriteLine($"Old phone: {client.PhoneNumber}");
        string value = AnsiConsole.Ask<string>("New phone: ");
        if (AnsiConsole.Confirm("Confirm changing phone"))
        {
            try
            {
                client.SetPhoneNumber(value);
            }
            catch (Exception err)
            {
                PrintError($"ERROR: {err}");
            }
        }

        return false;
    }

    private bool SetPassport(Client client)
    {
        AnsiConsole.WriteLine($"Old phone: {client.PhoneNumber}");
        string value = AnsiConsole.Ask<string>("New passport: ");
        if (AnsiConsole.Confirm("Confirm changing passport"))
        {
            try
            {
                client.SetPassportNumber(value);
            }
            catch (Exception err)
            {
                PrintError($"ERROR: {err}");
                if (AnsiConsole.Confirm("Remove passport instead?"))
                    client.RemovePassport();
            }
        }

        return false;
    }

    private bool SetAddress(Client client)
    {
        AnsiConsole.WriteLine($"Old address: {client.PhoneNumber}");
        string value = AnsiConsole.Ask<string>("New address: ");
        if (AnsiConsole.Confirm("Confirm changing address"))
        {
            try
            {
                var address = new Address(value);
                client.SetAddress(address);
            }
            catch (Exception err)
            {
                PrintError($"ERROR: {err}");
                if (AnsiConsole.Confirm("Remove address instead?"))
                    client.RemoveAddress();
            }
        }

        return false;
    }
}