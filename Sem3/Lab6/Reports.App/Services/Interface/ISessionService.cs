using Reports.App.Dto;

namespace Reports.App.Services;

public interface ISessionService
{
    Task<string> MakeSession(string login);
    Task<bool> ValidateToken(string token, string login);
    Task<EmployeeDto> EmployeeForToken(string token);
    Task<bool> RevokeToken(string token);
}