using Banks.Banking;
using Banks.Models;
using Spectre.Console;

namespace Banks.CLI;

public class AccountHandler : Handler
{
    public override bool Run()
    {
        if (CentralBank.Shared.Banks.Sum(b => b.Accounts.Count) == 0)
            return PrintError("No account are present");
        var account = SelectAccount("Select account");

        var choice = AnsiConsole.Prompt(new SelectionPrompt<string>()
            .Title("Select operation to perform")
            .AddChoices(new[]
            {
                "add", "withdraw", "transfer", "transactions", "cancel", "exit",
            }));
        switch (choice)
            {
                case "add":
                    return Add(account);
                case "withdraw":
                    return Withdraw(account);
                case "transfer":
                    return Transfer(account);
                case "transactions":
                    return ListTransactions(account);
                case "cancel":
                    return CancelTransaction(account);
                default:
                    return false;
            }
        }

    private bool Add(Account account)
    {
        var value = AnsiConsole.Ask<decimal>("Enter value");
        try
        {
            var transaction = account.InitiateAdd(value);
            transaction.Execute();
            PrintSuccess($"Transaction {transaction.Id} executed");
        }
        catch (Exception err)
        {
            PrintError($"ERROR: {err}");
        }

        return false;
    }

    private bool Withdraw(Account account)
    {
        var value = AnsiConsole.Ask<decimal>("Enter value");
        try
        {
            var transaction = account.InitiateWithdraw(value);
            transaction.Execute();
            PrintSuccess($"Transaction {transaction.Id} executed");
        }
        catch (Exception err)
        {
            PrintError($"ERROR: {err}");
        }

        return false;
    }

    private bool Transfer(Account account)
    {
        var another = SelectAccount("Select recipient");
        var value = AnsiConsole.Ask<decimal>("Enter value");
        try
        {
            var transaction = CentralBank.Shared.InitiateTransfer(account, another, value);
            transaction.Execute();
            PrintSuccess($"Transaction {transaction.Id} executed");
        }
        catch (Exception err)
        {
            PrintError($"ERROR: {err}");
        }

        return false;
    }

    private bool ListTransactions(Account account)
    {
        var table = new Table();
        table.AddColumns(new[]
        {
            "TYPE", "ID", "AMOUNT", "EXECUTED", "CANCELED",
        });

        foreach (var transaction in account.Transactions)
        {
            table.AddRow(new string[]
            {
                transaction.GetType().ToString().Replace("Transaction", string.Empty).Replace("Banks.Models", string.Empty),
                transaction.Id.ToString(),
                transaction.Amount.ToString(),
                transaction.IsExecuted ? "YES" : "NO",
                transaction.IsCanceled ? "YES" : "NO",
            });
        }

        AnsiConsole.Write(table);
        Wait();
        return false;
    }

    private bool CancelTransaction(Account account)
    {
        var choice = AnsiConsole.Prompt(new SelectionPrompt<string>()
            .Title("Select transaction to cancel")
            .AddChoices(account.Transactions
                .Where(t => t.IsExecuted && !t.IsCanceled)
                .Select(t => $"{t.Id} - {t.GetType().ToString().Replace("Transaction", string.Empty).Replace("Banks.Models", string.Empty)} {t.Amount}")));
        var transaction = account.Transactions.First(t => t.Id.ToString() == choice.Split(" ")[0]);
        try
        {
            transaction.Cancel();
        }
        catch (Exception err)
        {
            PrintError($"ERROR: {err}");
        }

        return false;
    }

    private Account SelectAccount(string prompt)
    {
        var accountChoice = AnsiConsole.Prompt(new SelectionPrompt<string>()
            .Title(prompt)
            .AddChoices(
                CentralBank.Shared.Banks
                    .SelectMany(b => b.Accounts)
                    .Select(a => $"{a.Id} - {a.GetType().ToString().Replace("Account", string.Empty)} @ {a.Bank.Name}")));
        return CentralBank.Shared.Banks.SelectMany(b => b.Accounts).First(a => a.Id.ToString() == accountChoice.Split(" ")[0]);
    }
}