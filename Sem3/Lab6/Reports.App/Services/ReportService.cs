using Microsoft.EntityFrameworkCore;
using Reports.App.Dto;
using Reports.App.Exceptions;
using Reports.App.Mapping;
using Reports.Data;
using Reports.Data.Enums;
using Reports.Models;

namespace Reports.App.Services;

public class ReportService : IReportService
{
    private AppDbContext _context;

    public ReportService(AppDbContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        _context = context;
    }

    public async Task<IReadOnlyList<ReportDto>> ListReports()
    {
        return await _context.Reports
            .Select(r => r.AsDto())
            .ToListAsync();
    }

    public async Task<IReadOnlyList<ReportDto>> ListReports(string supervisorLogin)
    {
        return await _context.Reports
            .Where(r => r.Author.Login == supervisorLogin)
            .Select(r => r.AsDto())
            .ToListAsync();
    }

    public async Task<IReadOnlyList<ReportDto>> ListReports(DateTime startTime, DateTime endTime)
    {
        return await _context.Reports
            .Where(r => r.StartTime >= startTime && r.EndTime <= endTime)
            .Select(r => r.AsDto())
            .ToListAsync();
    }

    public async Task<DetailedReportDto> GetDetailedReport(Guid id)
    {
        Report report = await _context.Reports
            .SingleOrDefaultAsync(r => r.Id == id);
        return report?.AsDetailedDto() ?? throw new ReportsNotFoundException($"Report with id {id} not found");
    }

    public async Task<DetailedReportDto> MakeReport(DateTime startTime, DateTime endTime, string supervisorLogin)
    {
        var actions = _context.Actions
            .Where(a => a.Timestamp >= startTime && a.Timestamp <= endTime);
        var statsByEmployee = new Dictionary<Guid, int>();

        Employee supervisor = _context.Employees.Single(e => e.Login == supervisorLogin);
        if (supervisor.Rank == EmployeeRank.Regular)
            throw new ReportsCredentialsException("Non-supervisor can't create reports");

        int totalHandled = 0;
        int subordinateHandled = 0;

        foreach (var action in actions)
        {
            totalHandled += 1;
            if (action.Account.Owner.Supervisor == supervisor)
                subordinateHandled += 1;
            if (!statsByEmployee.ContainsKey(action.Account.Id))
                statsByEmployee[action.Account.Id] = 1;
            else
                statsByEmployee[action.Account.Id] += 1;
        }

        var countByAccount = new List<ReportStat>();

        foreach (var kv in statsByEmployee)
        {
            var account = _context.Accounts.Single(a => a.Id == kv.Key);
            var reportStat = new ReportStat(account, kv.Value);
            countByAccount.Add(reportStat);
        }

        var report = new Report(
            Guid.NewGuid(),
            startTime,
            endTime,
            DateTime.Now,
            supervisor,
            totalHandled,
            subordinateHandled,
            countByAccount);

        await _context.Reports.AddAsync(report);
        await _context.SaveChangesAsync();
        return _context.Reports.Include(r => r.CountByAccount).Single(r => r.Id == report.Id).AsDetailedDto();
    }
}