using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace Reports.Models;

public class Report
{
    public Report(
        Guid id,
        DateTime startTime,
        DateTime endTime,
        DateTime creationTime,
        Employee author,
        int totalHandled,
        int subordinateHandled,
        List<ReportStat> countByAccount)
    {
        ArgumentNullException.ThrowIfNull(author);
        ArgumentNullException.ThrowIfNull(countByAccount);
        if (startTime > endTime)
            throw new ArgumentException($"Start time can't be after end time ({startTime} > {endTime})");
        if (endTime > creationTime)
            throw new ArgumentException($"Creation time can't be before end time ({endTime} > {creationTime})");
        (Id, StartTime, EndTime, CreationTime, Author, TotalHandled, SubordinateHandled, CountByAccount) = (id,
            startTime, endTime, creationTime, author, totalHandled, subordinateHandled, countByAccount);
    }

    protected Report() { }

    [Key]
    public Guid Id { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public DateTime CreationTime { get; set; }
    public virtual Employee Author { get; set; }

    public int TotalHandled { get; set; }
    public int SubordinateHandled { get; set; }
    public virtual List<ReportStat> CountByAccount { get; set; }
}