using Microsoft.EntityFrameworkCore;
using Reports.App.Dto;
using Reports.App.Exceptions;
using Reports.App.Mapping;
using Reports.Data;
using Reports.Models;

namespace Reports.App.Services;

public class AccountService : IAccountService
{
    private AppDbContext _context;

    public AccountService(AppDbContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        _context = context;
    }

    public async Task<AccountDto> MakeAccount(string id, string serviceId, string ownerLogin)
    {
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(serviceId);
        if (await _context.Accounts.AnyAsync(a => a.Name == id && a.ServiceId == serviceId))
            throw new ReportsDuplicateException($"Account with id {id} already exists in service {serviceId}");
        Employee owner = null;
        if (ownerLogin is not null)
        {
            owner = await _context.Employees.SingleOrDefaultAsync(e => e.Login == ownerLogin);
            if (owner is null)
                throw new ReportsNotFoundException($"Owner with login {ownerLogin} not found");
        }

        var account = new Account(id, serviceId, owner);
        await _context.Accounts.AddAsync(account);
        await _context.SaveChangesAsync();
        return account.AsDto();
    }

    public async Task<IReadOnlyList<AccountDto>> FindByOwner(string ownerLogin)
    {
        return await _context.Accounts
            .Where(a => a.Owner.Login == ownerLogin)
            .Select(a => a.AsDto())
            .ToListAsync();
    }

    public async Task<IReadOnlyList<AccountDto>> FindByService(string serviceId)
    {
        return await _context.Accounts
            .Where(a => a.ServiceId == serviceId)
            .Select(a => a.AsDto())
            .ToListAsync();
    }

    public async Task<AccountDto> FindAccount(string id, string serviceId)
    {
        Account account = await _context.Accounts.SingleOrDefaultAsync(a => a.Name == id && a.ServiceId == serviceId);
        return account?.AsDto();
    }
}