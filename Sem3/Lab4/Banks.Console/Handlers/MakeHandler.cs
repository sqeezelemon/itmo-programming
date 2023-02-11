using Banks.Banking;
using Banks.Models;
using Spectre.Console;

namespace Banks.CLI;

public class MakeHandler : Handler
{
    public override bool Run()
    {
        var choice = AnsiConsole.Prompt(new SelectionPrompt<string>()
            .Title("Select operation to perform")
            .AddChoices(new[]
            {
                "account", "bank", "client", "exit",
            }));
        switch (choice)
        {
            case "account":
                return MakeAccount();
            case "bank":
                return MakeBank();
            case "client":
                return MakeClient();
            default:
                return false;
        }
    }

    private bool MakeBank()
    {
        AnsiConsole.WriteLine("Creating a bank");
        var name = AnsiConsole.Ask<string>("Name:");
        Bank bank = CentralBank.Shared.RegisterBank(name, Program.Default);
        Program.LastBank = bank;
        return PrintSuccess($"{bank.Name} was successfully created, use [bold]bank settings[/] to modify from defaults.");
    }

    private bool MakeClient()
    {
        AnsiConsole.WriteLine("Setup Client");
        var name = AnsiConsole.Ask<string>("Name:");
        var phone = AnsiConsole.Ask<string>("Phone:");
        AnsiConsole.WriteLine("Optional parameters (press enter to skip)");
        var passport = AnsiConsole.Prompt(
            new TextPrompt<string>("Passport:")
                .AllowEmpty());
        var addressStr = AnsiConsole.Prompt(
            new TextPrompt<string>("Address:")
                .AllowEmpty());
        Address? address = string.IsNullOrWhiteSpace(addressStr) ? null : new Address(addressStr);

        Client client;
        try
        {
            client = new Client(name, phone, passport, address);
            Program.Clients.Add(client);
        }
        catch (Exception err)
        {
            return PrintError($"ERROR: {err}");
        }

        foreach (var bank in CentralBank.Shared.Banks)
        {
            try
            {
                bank.AcceptClient(client);
            }
            catch
            {
            }
        }

        return PrintSuccess("Client was successfully created.");
    }

    private bool MakeAccount()
    {
        if (CentralBank.Shared.Banks.Count() == 0)
            return PrintError("No available banks");

        var bankName = AnsiConsole.Prompt<string>(new SelectionPrompt<string>()
            .Title("Select bank")
            .AddChoices(CentralBank.Shared.Banks.Select(b => b.Name).ToArray()));
        var bank = CentralBank.Shared.Banks.First(b => b.Name == bankName);

        if (bank.Clients.Count() == 0)
            return PrintError("No available clients");
        var clientChoice = AnsiConsole.Prompt<string>(new SelectionPrompt<string>()
            .Title("Select client")
            .AddChoices(bank.Clients.Select(c => c.PhoneNumber).ToArray()));
        Client client = bank.Clients.First(c => c.PhoneNumber == clientChoice);

        var choice = AnsiConsole.Prompt(new SelectionPrompt<string>()
            .Title("Select account type")
            .AddChoices(new[]
            {
                "debit", "credit", "deposit",
            }));
        try
        {
            bank.AcceptClient(client);
        }
        catch
        {
        }

        Account account;
        switch (choice)
        {
            case "debit":
                account = bank.MakeDebitAccount(client);
                return PrintSuccess($"Debit account with ID {account.Id} created");
            case "credit":
                account = bank.MakeCreditAccount(client);
                return PrintSuccess($"Credit account with ID {account.Id} created");
            case "deposit":
                var months = AnsiConsole.Ask<int>("Deposit interval");
                account = bank.MakeDepositAccount(client, DateTime.Now.AddMonths(months));
                return PrintSuccess($"Deposit account with ID {account.Id} created");
            default:
                break;
        }

        return false;
    }
}