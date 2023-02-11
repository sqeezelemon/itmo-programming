using System.Net;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Reports.App.Dto;
using Reports.App.Exceptions;
using Reports.App.Services;
using Reports.Models;

namespace Reports.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthenticationController : ControllerBase
{
    private IEmployeeService employeeService;
    private ISessionService sessionService;

    public AuthenticationController(IEmployeeService eService, ISessionService sService)
    {
        ArgumentNullException.ThrowIfNull(eService);
        ArgumentNullException.ThrowIfNull(sService);
        (employeeService, sessionService) = (eService, sService);
        
    }
    
    [HttpPost("login")]
    public async Task<IActionResult> Login(string login, string password)
    {
        EmployeeDto account;
        try
        {
            account = await employeeService.FindEmployee(login, password);
        }
        catch (ReportsCredentialsException)
        {
            return Forbid("Incorrect password");
        }
        catch (ReportsNotFoundException)
        {
            return NotFound($"Account {login} not found");
        }

        var sessionToken = await sessionService.MakeSession(login);

        var claims = new Claim[]
        {
            new Claim(ClaimTypes.Name, account.login),
            new Claim(ClaimTypes.Sid, sessionToken),
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

        return Ok(account);
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout(string token)
    {
        try
        {
            await sessionService.RevokeToken(token);
        }
        catch (ReportsNotFoundException)
        {
            return NotFound("Token not found");
        }

        return Ok();
    }
}