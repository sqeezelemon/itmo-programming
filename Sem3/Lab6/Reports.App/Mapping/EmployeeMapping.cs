using Reports.App.Dto;
using Reports.Models;

namespace Reports.App.Mapping;

public static class EmployeeMapping
{
    public static EmployeeDto AsDto(this Employee employee)
    {
        if (employee.Supervisor is null)
            return new EmployeeDto(employee.Name, employee.Login, null);
        return new EmployeeDto(employee.Name, employee.Login, employee.Supervisor.Login);
    }
}