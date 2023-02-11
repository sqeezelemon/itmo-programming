using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Reports.App.Dto;
using Reports.App.Exceptions;
using Reports.App.Services;

namespace Reports.Api.Extensions;

public static class SessionChecker
{
    public static async Task<IActionResult> CheckSession(this ControllerBase controller, ISessionService sessionService)
    {
        var sid = controller.HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Sid);
        if (sid is null)
            return controller.Unauthorized("Not logged in");
        EmployeeDto employee = null;
        try
        {
            employee = await sessionService.EmployeeForToken(sid.Value);
        }
        catch (ReportsNotFoundException)
        {
            return controller.Unauthorized("Invalid authentication token");
        }
        var name = controller.HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name);
        if (name == null || employee.login != name?.Value)
            return controller.BadRequest("Corrupted login cookie");

        return null;
    }

    public static async Task<EmployeeDto> GetEmployeeFromContext(this ControllerBase controller, ISessionService sessionService)
    {
        var sid = controller.HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Sid);
        return await sessionService.EmployeeForToken(sid.Value);
    }
}