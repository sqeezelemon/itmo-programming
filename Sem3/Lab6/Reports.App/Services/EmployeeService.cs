using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Reports.App.Dto;
using Reports.App.Exceptions;
using Reports.App.Mapping;
using Reports.Data;
using Reports.Data.Enums;
using Reports.Models;

namespace Reports.App.Services;

public class EmployeeService : IEmployeeService
{
    private AppDbContext _context;

    public EmployeeService(AppDbContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        _context = context;
    }

    public async Task<EmployeeDto> FindEmployee(string login, string password)
    {
        Employee employee = await _context.Employees.SingleOrDefaultAsync(e => e.Login == login);
        if (employee is null)
            throw new ReportsNotFoundException($"Employee with login {employee} not found");
        if (employee.PasswordHash != GetPasswordHash(password))
            throw new ReportsCredentialsException("Incorrect password");
        return employee.AsDto();
    }

    public async Task<EmployeeDto> FindEmployee(string login)
    {
        Employee employee = await _context.Employees.SingleOrDefaultAsync(e => e.Login == login);
        if (employee is null)
            throw new ReportsNotFoundException($"Employee with login {employee} not found");
        return employee.AsDto();
    }

    public async Task<EmployeeDto> MakeEmployee(string login, string password, string name, string supervisorLogin, EmployeeRank rank)
    {
        if (await _context.Employees.AnyAsync(e => e.Login == login))
            throw new ReportsDuplicateException($"Employee with login {login} already exists");
        Employee supervisor = null;
        if (supervisorLogin is not null)
        {
            supervisor = await _context.Employees.SingleOrDefaultAsync(e => e.Login == supervisorLogin);
            if (supervisor is null)
                throw new ReportsNotFoundException($"Employee with login {supervisorLogin} does not exist");
        }

        Employee employee = new Employee(name, supervisor, login, GetPasswordHash(password), rank);
        await _context.Employees.AddAsync(employee);
        await _context.SaveChangesAsync();
        return employee.AsDto();
    }

    public async Task<IReadOnlyList<EmployeeDto>> FindSubordinates(string supervisorLogin)
    {
        return await _context.Employees
            .Where(e => e.Supervisor.Login == supervisorLogin)
            .Select(e => e.AsDto())
            .ToListAsync();
    }

    public async Task<EmployeeDto> ChangeRank(string login, EmployeeRank newRank)
    {
        Employee employee = await _context.Employees.SingleOrDefaultAsync(e => e.Login == login);
        if (employee is null)
            throw new ReportsNotFoundException($"Employee with login {employee} not found");
        employee.Rank = newRank;
        await _context.SaveChangesAsync();
        return employee.AsDto();
    }

    public async Task<EmployeeDto> ChangeSupervisor(string login, string newSupLogin)
    {
        Employee employee = await _context.Employees.SingleOrDefaultAsync(e => e.Login == login);
        if (employee is null)
            throw new ReportsNotFoundException($"Employee with login {login} not found");
        Employee supervisor = null;
        if (newSupLogin is not null)
        {
            supervisor = await _context.Employees.SingleOrDefaultAsync(e => e.Login == newSupLogin);
            if (supervisor is null)
                throw new ReportsNotFoundException($"Employee with login {newSupLogin} not found");
            if (supervisor.Supervisor.Login == employee.Login || login == newSupLogin)
                throw new ReportsHierarchyException("Circular reference in supervisor field attempted");
        }

        employee.Supervisor = supervisor;
        await _context.SaveChangesAsync();
        return employee.AsDto();
    }

    private string GetPasswordHash(string password)
    {
        // Честно украдено у Ромы :)
        using var hashingAlgorithm = SHA256.Create();
        byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
        return BitConverter.ToString(hashingAlgorithm.ComputeHash(passwordBytes));
    }
}