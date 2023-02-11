using Microsoft.AspNetCore.Mvc;
using Reports.Api.Extensions;
using Reports.App.Exceptions;
using Reports.App.Services;

namespace Reports.Api.Controllers;

[ApiController]
[Route("api/report")]
public class ReportsController : ControllerBase
{
    private IEmployeeService employeeService;
    private ISessionService sessionService;
    private IReportService reportService;

    public ReportsController(IEmployeeService eService, ISessionService sService, IReportService rService)
    {
        ArgumentNullException.ThrowIfNull(eService);
        ArgumentNullException.ThrowIfNull(sService);
        ArgumentNullException.ThrowIfNull(rService);
        (employeeService, sessionService, reportService) = (eService, sService, rService);
        
    }

    [HttpGet("all")]
    public async Task<IActionResult> ListReports()
    {
        var sessionVerdict = await this.CheckSession(sessionService);
        if (sessionVerdict is not null)
            return sessionVerdict;

        return Ok(reportService.ListReports());
    }
    
    [HttpGet("{username}/all")]
    public async Task<IActionResult> ListReports(string username)
    {
        var sessionVerdict = await this.CheckSession(sessionService);
        if (sessionVerdict is not null)
            return sessionVerdict;

        return Ok(reportService.ListReports(username));
    }
    
    [HttpPost("make")]
    public async Task<IActionResult> Make(DateTime start, DateTime end)
    {
        var sessionVerdict = await this.CheckSession(sessionService);
        if (sessionVerdict is not null)
            return sessionVerdict;

        var employee = (await this.GetEmployeeFromContext(sessionService)).login;

        try
        {
            var report = await reportService.MakeReport(start, end, employee);
            return Ok(report);
        }
        catch (ReportsCredentialsException e)
        {
            return Unauthorized(e.Message);
        } 
    }
    
    [HttpGet("{reportId}")]
    public async Task<IActionResult> GetReport(string reportId)
    {
        var sessionVerdict = await this.CheckSession(sessionService);
        if (sessionVerdict is not null)
            return sessionVerdict;

        try
        {
            var report = await reportService.GetDetailedReport(Guid.Parse(reportId));
            return Ok(report);
        }
        catch (ReportsNotFoundException e)
        {
            return NotFound(e.Message);
        }
    }
}