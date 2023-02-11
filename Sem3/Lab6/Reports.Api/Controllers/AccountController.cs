using Microsoft.AspNetCore.Mvc;
using Reports.Api.Extensions;
using Reports.App.Exceptions;
using Reports.App.Services;

namespace Reports.Api.Controllers;

[ApiController]
[Route("api/account")]
public class AccountController : ControllerBase
{
    private IEmployeeService employeeService;
    private ISessionService sessionService;
    private IAccountService accountService;
    
    public AccountController(IEmployeeService eService, ISessionService sService, IAccountService aService)
    {
        ArgumentNullException.ThrowIfNull(eService);
        ArgumentNullException.ThrowIfNull(sService);
        ArgumentNullException.ThrowIfNull(aService);
        (employeeService, sessionService, accountService) = (eService, sService, aService);
    }
    
    [HttpGet("{service}/{username}")]
    public async Task<IActionResult> GetAccount(string service, string username)
    {
        var sessionVerdict = await this.CheckSession(sessionService);
        if (sessionVerdict is not null)
            return sessionVerdict;

        try
        {
            var account = await accountService.FindAccount(username, service);
            return Ok(account);
        }
        catch (ReportsNotFoundException e)
        {
            return NotFound(e.Message);
        }
    }
    
    [HttpPost("{serviceId}/make")]
    public async Task<IActionResult> Make(string serviceId, string username, string employee)
    {
        var sessionVerdict = await this.CheckSession(sessionService);
        if (sessionVerdict is not null)
            return sessionVerdict;

        if (employee == "me" || employee == null)
        {
            employee = (await this.GetEmployeeFromContext(sessionService)).login;
        }
        else
        {
            try
            {
                await employeeService.FindEmployee(employee);
            }
            catch (ReportsNotFoundException e)
            {
                return NotFound(e.Message);
            }
        }

        try
        {
            var account = await accountService.MakeAccount(username, serviceId, employee);
            return Ok(account);
        }
        catch (ReportsNotFoundException e)
        {
            return NotFound(e.Message);
        }
    }
}