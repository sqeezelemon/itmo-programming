using Reports.App.Dto;
using Reports.Data.Enums;

namespace Reports.App.Services;

public interface IEmployeeService
{
    Task<EmployeeDto> FindEmployee(string login);
    Task<EmployeeDto> FindEmployee(string login, string password);
    Task<EmployeeDto> MakeEmployee(string login, string password, string name, string supervisorLogin, EmployeeRank rank);
    Task<IReadOnlyList<EmployeeDto>> FindSubordinates(string supervisorLogin);
    Task<EmployeeDto> ChangeRank(string login, EmployeeRank newRank);
    Task<EmployeeDto> ChangeSupervisor(string login, string newSupLogin);
}