using Reports.Data.Enums;
using SQLitePCL;

namespace Reports.Models;

public class Employee
{
    public Employee(string name, Employee supervisor, string login, string passwordHash, EmployeeRank rank)
    {
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(login);
        ArgumentNullException.ThrowIfNull(passwordHash);
        (Name, Supervisor, Login, PasswordHash, Rank) = (name, supervisor, login, passwordHash, rank);
    }

    protected Employee() { }

    public string Name { get; set; }
    public virtual Employee Supervisor { get; set; }
    public string Login { get; set; }
    public string PasswordHash { get; set; }
    public EmployeeRank Rank { get; set; }
}