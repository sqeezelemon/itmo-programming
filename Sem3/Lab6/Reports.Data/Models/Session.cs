namespace Reports.Models;

public class Session
{
    public Session(Employee employee, string token)
    {
        ArgumentNullException.ThrowIfNull(employee);
        (Employee, Token) = (employee, token);
    }

    protected Session() { }

    public virtual Employee Employee { get; set; }
    public string Token { get; set; }
}