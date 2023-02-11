using Microsoft.EntityFrameworkCore;
using Reports.App.Dto;
using Reports.App.Exceptions;
using Reports.App.Mapping;
using Reports.Data;
using Reports.Models;

namespace Reports.App.Services;

public class SessionService : ISessionService
{
    private AppDbContext _context;

    public SessionService(AppDbContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        _context = context;
    }

    public async Task<string> MakeSession(string login)
    {
        Employee employee = await _context.Employees.SingleOrDefaultAsync(e => e.Login == login);
        if (employee is null)
            throw new ReportsNotFoundException($"Employee {login} not found");
        var session = new Session(employee, GenerateToken());
        await _context.Sessions.AddAsync(session);
        await _context.SaveChangesAsync();
        return session.Token;
    }

    public async Task<bool> ValidateToken(string token, string login)
    {
        var session = await _context.Sessions.FindAsync(token);
        if (session is null)
            throw new ReportsNotFoundException("Session not found");
        if (session.Employee.Login != login)
            throw new ReportsCredentialsException($"Token owner mismatch (belongs to {session.Employee.Login})");
        return true;
    }

    public async Task<EmployeeDto> EmployeeForToken(string token)
    {
        Session session = await _context.Sessions.FindAsync(token);
        if (session is null)
            throw new ReportsNotFoundException("Session not found");
        return session.Employee.AsDto();
    }

    public async Task<bool> RevokeToken(string token)
    {
        var session = await _context.Sessions.FindAsync(token);
        if (session is null)
            throw new ReportsNotFoundException("Session not found");
        _context.Sessions.Remove(session);
        return true;
    }

    private string GenerateToken() => Guid.NewGuid().ToString();
}