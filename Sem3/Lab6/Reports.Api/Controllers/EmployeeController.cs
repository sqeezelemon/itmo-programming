using Microsoft.AspNetCore.Mvc;
using Reports.Api.Extensions;
using Reports.App.Exceptions;
using Reports.App.Services;
using Reports.Data.Enums;

namespace Reports.Api.Controllers;

[ApiController]
[Route("api/employee")]
public class EmployeeController : ControllerBase
{
    private IEmployeeService employeeService;
    private ISessionService sessionService;
    private IAccountService accountService;

    private static readonly string[] reservedUsernames = new string[]
    {
        "me", "make", "subordinates", "all"
    };

    public EmployeeController(IEmployeeService eService, ISessionService sService, IAccountService aService)
    {
        ArgumentNullException.ThrowIfNull(eService);
        ArgumentNullException.ThrowIfNull(sService);
        ArgumentNullException.ThrowIfNull(aService);
        (employeeService, sessionService, accountService) = (eService, sService, aService);
    }

    [HttpGet("{employee}/subordinates")]
    public async Task<IActionResult> ListSubordinates(string employee)
    {
        var sessionVerdict = await this.CheckSession(sessionService);
        if (sessionVerdict is not null)
            return sessionVerdict;
        
        if (employee == "me")
            employee = (await this.GetEmployeeFromContext(sessionService)).login;

        var result = await employeeService.FindSubordinates(employee);
        return Ok(result);
    }

    [HttpGet("{employee}")]
    public async Task<IActionResult> ViewEmployee(string employee)
    {
        var sessionVerdict = await this.CheckSession(sessionService);
        if (sessionVerdict is not null)
            return sessionVerdict;
        
        if (employee == "me")
            employee = (await this.GetEmployeeFromContext(sessionService)).login;

        try
        {
            var result = await employeeService.FindEmployee(employee);
            return Ok(result);
        }
        catch (ReportsNotFoundException)
        {
            return NotFound("Employee not found");
        }
    }
    
    [HttpGet("{employee}/accounts")]
    public async Task<IActionResult> ListAccounts(string employee)
    {
        var sessionVerdict = await this.CheckSession(sessionService);
        if (sessionVerdict is not null)
            return sessionVerdict;
        
        if (employee == "me")
            employee = (await this.GetEmployeeFromContext(sessionService)).login;

        var result = await accountService.FindByOwner(employee);
        return Ok(result);
    }

    [HttpPost("make")]
    public async Task<IActionResult> MakeEmployee(string login, string password, string name, EmployeeRank rank, string supervisor = null)
    {
        // var sessionVerdict = await this.CheckSession(sessionService);
        // if (sessionVerdict is not null)
        //     return sessionVerdict;

        if (reservedUsernames.Contains(login))
            return Conflict("Username is reserved");

        try
        {
            var employee = await employeeService.MakeEmployee(login, password, name, supervisor, rank);
            return Ok(employee);
        }
        catch (ReportsDuplicateException e)
        {
            return Conflict(e.Message);
        }
        catch (ReportsNotFoundException e)
        {
            return NotFound(e.Message);
        }
    }

}