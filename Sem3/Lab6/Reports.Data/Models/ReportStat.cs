using System.Linq.Expressions;

namespace Reports.Models;

public class ReportStat
{
    public ReportStat(Account account, int count)
    {
        ArgumentNullException.ThrowIfNull(account);
        (Account, Count) = (account, count);
    }

    protected ReportStat() { }

    public Guid Id { get; set; } = Guid.NewGuid();
    public virtual Report Report { get; set; }
    public virtual Account Account { get; set; }
    public int Count { get; set; }
}