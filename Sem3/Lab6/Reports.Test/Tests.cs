using Microsoft.EntityFrameworkCore;
using Reports.App.Dto;
using Reports.App.Exceptions;
using Reports.App.Services;
using Reports.Data;
using Reports.Data.Enums;
using Xunit;

namespace Reports.Test;

public class ReportsTest : IDisposable
{
    private readonly AppDbContext context;

    public ReportsTest()
    {
        var optsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optsBuilder.UseSqlite("Filename=Tests.db");
        DbContextOptions<AppDbContext> opts = optsBuilder.Options;
        context = new AppDbContext(opts);
    }

    [Fact]
    public async void AccountService()
    {
        ClearTable();
        var employeeService = new EmployeeService(context);
        EmployeeDto supervisor = await employeeService.MakeEmployee("sup", "pwd", "Suppa Supervisor", null, EmployeeRank.Supervisor);
        EmployeeDto employee1 = await employeeService.MakeEmployee("em1", "pwd", "Emma Emplovich", supervisor.login, EmployeeRank.Regular);
        EmployeeDto employee2 = await employeeService.MakeEmployee("em2", "pwd", "Empo Emploev", supervisor.login, EmployeeRank.Regular);

        var service = new AccountService(context);
        await service.MakeAccount("sup", "service1", "sup");
        await service.MakeAccount("sup", "service2", "sup");
        await service.MakeAccount("sup2", "service1", "sup");
        await service.MakeAccount("em1", "service1", "em1");

        IReadOnlyList<AccountDto> accounts = await service.FindByOwner("sup");
        Assert.Equal(3, accounts.Count);

        accounts = await service.FindByService("service1");
        Assert.Equal(3, accounts.Count);
    }

    [Fact]
    public async void EmployeeService()
    {
        ClearTable();
        var service = new EmployeeService(context);
        EmployeeDto supervisor = await service.MakeEmployee("sup", "pwd", "Suppa Supervisor", null, EmployeeRank.Supervisor);
        EmployeeDto employee1 = await service.MakeEmployee("em1", "pwd", "Emma Emplovich", supervisor.login, EmployeeRank.Regular);
        EmployeeDto employee2 = await service.MakeEmployee("em2", "pwd", "Empo Emploev", supervisor.login, EmployeeRank.Regular);

        IReadOnlyList<EmployeeDto> subordinates = await service.FindSubordinates(supervisor.login);
        Assert.Equal(2, subordinates.Count());

        subordinates = await service.FindSubordinates(employee1.login);
        Assert.Empty(subordinates);

        await service.ChangeSupervisor(employee2.login, employee1.login);
        subordinates = await service.FindSubordinates(employee1.login);
        Assert.Single(subordinates);

        // Shouldn't throw
        await service.FindEmployee("sup", "pwd");
        // Invalid password
        await Assert.ThrowsAsync<ReportsCredentialsException>(() => service.FindEmployee("sup", "pwdd"));
    }

    [Fact]
    public async void SessionService()
    {
        ClearTable();
        var employeeService = new EmployeeService(context);
        EmployeeDto supervisor = await employeeService.MakeEmployee("sup", "pwd", "Suppa Supervisor", null, EmployeeRank.Supervisor);
        EmployeeDto employee1 = await employeeService.MakeEmployee("em1", "pwd", "Emma Emplovich", supervisor.login, EmployeeRank.Regular);
        EmployeeDto employee2 = await employeeService.MakeEmployee("em2", "pwd", "Empo Emploev", supervisor.login, EmployeeRank.Regular);

        var service = new SessionService(context);
        var token = await service.MakeSession("sup");

        // Incorrect password
        await Assert.ThrowsAsync<ReportsCredentialsException>(() => service.ValidateToken(token, "em1"));

        var employee = await service.EmployeeForToken(token);
        Assert.Equal("sup", employee.login);
    }

    [Fact]
    public async void MessageService()
    {
        ClearTable();
        var employeeService = new EmployeeService(context);
        EmployeeDto supervisor = await employeeService.MakeEmployee("sup", "pwd", "Suppa Supervisor", null, EmployeeRank.Supervisor);
        EmployeeDto employee1 = await employeeService.MakeEmployee("em1", "pwd", "Emma Emplovich", supervisor.login, EmployeeRank.Regular);
        EmployeeDto employee2 = await employeeService.MakeEmployee("em2", "pwd", "Empo Emploev", supervisor.login, EmployeeRank.Regular);

        var accountService = new AccountService(context);
        await accountService.MakeAccount("em1", "test", "em1");

        var service = new MessageService(context, accountService);
        service.AddMessageSource("test", new MessageGenerator("em1"));
        
        // 2 Messages sent by MessageGenerator
        var messages =  await service.FetchNewMessages();
        Assert.Equal(2, messages.Count);
        
        // 1 existing account + 2 accounts from sent messages = 3
        var accounts = await accountService.FindByService("test");
        Assert.Equal(3, accounts.Count);

        // Sent a message, but only 2 are unhandled
        await service.SendMessage("test", "em1", "sender1", "Lorem?");
        messages = await service.GetUnhandledMessages("em1");
        Assert.Equal(2, messages.Count);

        // 1 handled, 1 left
        await service.HandleMessage(messages[0].id, "em1");
        messages = await service.GetUnhandledMessages("em1");
        Assert.Single(messages);
        service.RemoveMessageSource("test");
    }

    [Fact]
    public async void ReportService()
    {
        ClearTable();
        var employeeService = new EmployeeService(context);
        EmployeeDto supervisor = await employeeService.MakeEmployee("sup", "pwd", "Suppa Supervisor", null, EmployeeRank.Supervisor);
        EmployeeDto employee1 = await employeeService.MakeEmployee("em1", "pwd", "Emma Emplovich", supervisor.login, EmployeeRank.Regular);
        EmployeeDto employee2 = await employeeService.MakeEmployee("em2", "pwd", "Empo Emploev", supervisor.login, EmployeeRank.Regular);

        var accountService = new AccountService(context);
        await accountService.MakeAccount("em1", "test", "em1");

        var messageService = new MessageService(context, accountService);
        messageService.AddMessageSource("test", new MessageGenerator("em1"));

        var service = new ReportService(context);

        var messages =  await messageService.FetchNewMessages();
        await messageService.SendMessage("test", "em1", "sender1", "Lorem?");
        await messageService.HandleMessage(messages[0].id, "em1");

        var report = await service.MakeReport(DateTime.MinValue, DateTime.Now, "sup");
        Assert.Equal(1, report.totalHandled);
        Assert.Equal(1, report.countByAccount.Single(c => c.account.name == "em1").count);
        messageService.RemoveMessageSource("test");
    }

    public void Dispose() => context.Dispose();

    private void ClearTable()
    {
        try
        {
            context.Sessions.RemoveRange(context.Sessions);
        }
        catch { }
        
        try
        {
            context.Employees.RemoveRange(context.Employees);
        }
        catch { }
        
        try
        {
            context.ReportStats.RemoveRange(context.ReportStats);
        }
        catch { }
        
        try
        {
            context.Reports.RemoveRange(context.Reports);
        }
        catch { }
        
        try
        {
            context.Messages.RemoveRange(context.Messages);
        }
        catch { }
        
        try
        { 
            context.Accounts.RemoveRange(context.Accounts);
        }
        catch { }

        try
        {
            context.Actions.RemoveRange(context.Actions);
        }
        catch { }
        
        context.SaveChanges();
    }
}