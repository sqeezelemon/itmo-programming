using Microsoft.AspNetCore.Mvc;
using Reports.Api.Extensions;
using Reports.App.Exceptions;
using Reports.App.Services;

namespace Reports.Api.Controllers;

[ApiController]
[Route("api/message")]
public class MessageController : ControllerBase
{
    private IMessageService messageService;
    private ISessionService sessionService;
    private IAccountService accountService;
    
    public MessageController(IMessageService mService, ISessionService sService, IAccountService aService)
    {
        ArgumentNullException.ThrowIfNull(mService);
        ArgumentNullException.ThrowIfNull(sService);
        ArgumentNullException.ThrowIfNull(aService);
        (messageService, sessionService, accountService) = (mService, sService, aService);
    }

    [HttpGet("new")]
    public async Task<IActionResult> GetNew(string accountId)
    {
        var sessionVerdict = await this.CheckSession(sessionService);
        if (sessionVerdict is not null)
            return sessionVerdict;

        try
        {
            var messages = await messageService.FetchNewMessages();
            return Ok(messages);
        }
        catch (ReportsNotFoundException e)
        {
            return NotFound(e.Message);
        }
    }

    [HttpGet("unhandled")]
    public async Task<IActionResult> Unhandled()
    {
        var sessionVerdict = await this.CheckSession(sessionService);
        if (sessionVerdict is not null)
            return sessionVerdict;

        var employee = (await this.GetEmployeeFromContext(sessionService)).login;
        var services = (await accountService.FindByOwner(employee))
            .GroupBy(a => a.serviceId)
            .Select(p => p.Key)
            .ToHashSet();
        var messages = (await messageService.GetUnhandledMessages(employee))
            .Where(m => services.Contains(m.serviceId))
            .ToList();
        return Ok(messages);
    }

    [HttpPost("{serviceId}/new")]
    public async Task<IActionResult> MakeNew(string serviceId, string senderId, string recipientId, [FromBody] string value)
    {
        var sessionVerdict = await this.CheckSession(sessionService);
        if (sessionVerdict is not null)
            return sessionVerdict;

        var employee = (await this.GetEmployeeFromContext(sessionService)).login;
        var account = await accountService.FindAccount(serviceId, senderId);
        if (account.ownerLogin != employee)
            return BadRequest("Account does not belong to the current user");

        try
        {
            var message = messageService.SendMessage(serviceId, senderId, recipientId, value);
            return Ok(message);
        }
        catch (ReportsNotFoundException e)
        {
            return NotFound(e.Message);
        }
    }

    [HttpPost("{serviceId}/{messageId}/handle")]
    public async Task<IActionResult> Handle(string serviceId, string messageId, string accountId)
    {
        var sessionVerdict = await this.CheckSession(sessionService);
        if (sessionVerdict is not null)
            return sessionVerdict;

        var employee = (await this.GetEmployeeFromContext(sessionService)).login;
        var account = await accountService.FindAccount(accountId, serviceId);
        if (account.ownerLogin != employee)
            return BadRequest("Account does not belong to the current user");

        var message = await messageService.HandleMessage(Guid.Parse(messageId), account.id.ToString());
        return Ok(message);
    }
}