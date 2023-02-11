using Banks.Banking;
using Spectre.Console;

namespace Banks.CLI;
/*
 * bank
 * - clients
 * - accounts
 * - set [BankSettings]
 */
public class BankHandler : Handler
{
    private Bank bank = null !;

    public override bool Run()
    {
        if (CentralBank.Shared.Banks.Count == 0)
        {
            PrintError("No banks were found");
            return false;
        }

        var bankName = AnsiConsole.Prompt<string>(new SelectionPrompt<string>()
            .Title("Select bank")
            .AddChoices(CentralBank.Shared.Banks.Select(b => b.Name).ToArray()));
        bank = CentralBank.Shared.Banks.First(b => b.Name == bankName);

        var choice = AnsiConsole.Prompt(new SelectionPrompt<string>()
            .Title("Select operation to perform")
            .AddChoices(new[]
            {
                "client", "accounts", "settings", "exit",
            }));
        switch (choice)
        {
            case "clients":
                return ListClients(bank);
            case "accounts":
                return ListAccounts(bank);
            case "settings":
                return Settings(bank);
            default:
                return false;
        }
    }

    private bool ListClients(Bank bank)
    {
        var table = new Table();
        table.Border(TableBorder.Rounded);
        table.AddColumns(new[]
        {
            "PHONE", "NAME", "PASSPORT", "ADDRESS",
        });
        foreach (var client in bank.Clients)
        {
            table.AddRow(new string[]
            {
                client.PhoneNumber,
                client.Name,
                client.PassportNumber,
                client.Address == null ? string.Empty : client.Address.Value,
            });
        }

        AnsiConsole.Write(table);
        Wait();
        return false;
    }

    private bool ListAccounts(Bank bank)
    {
        var table = new Table();
        table.Border(TableBorder.Rounded);
        table.AddColumns(new[]
        {
            "TYPE", "BALANCE", "PHONE", "BANK",
        });
        foreach (var account in bank.Accounts)
        {
            table.AddRow(new string[]
            {
                account.GetType().ToString().Replace("Account", string.Empty).Replace("Banks.Models", string.Empty),
                account.Balance.ToString(),
                account.Client.PhoneNumber,
                account.Bank.Name,
            });
        }

        AnsiConsole.Write(table);
        Wait();
        return false;
    }

    private bool Settings(Bank bank)
    {
        var choice = AnsiConsole.Prompt(new SelectionPrompt<string>()
            .Title("Select setting to modify")
            .AddChoices(new[]
            {
                "credit commission",
                "debit cashback",
                "transfer limit (suspicious clients)",
                "withdraw limit (suspicious clients)",
            }));
        decimal value;
        switch (choice)
        {
            case "credit commission":
                Console.WriteLine($"Old commission: {bank.Settings.CreditComission}");
                value = AnsiConsole.Ask<decimal>("New commission: ");
                bank.Settings.SetCreditComission(value);
                return false;
            case "debit cashback":
                Console.WriteLine($"Old cashback: {bank.Settings.CreditComission}");
                value = AnsiConsole.Ask<decimal>("New cashback: ");
                bank.Settings.SetDebitCashback(value);
                return false;
            case "transfer limit (suspicious clients)":
                Console.WriteLine($"Old limit: {bank.Settings.SuspiciousTransferLimit}");
                value = AnsiConsole.Ask<decimal>("New limit: ");
                bank.Settings.SetSuspiciousTransferLimit(value);
                return false;
            case "withdraw limit (suspicious clients)":
                Console.WriteLine($"Old limit: {bank.Settings.SuspiciousWithdrawLimit}");
                value = AnsiConsole.Ask<decimal>("New limit: ");
                bank.Settings.SetSuspiciousWithdrawLimit(value);
                return false;
            default:
                return false;
        }
    }
}