using Reports.App.Dto;
using Reports.Models;

namespace Reports.App.Mapping;

public static class ReportMapping
{
    public static ReportDto AsDto(this Report report)
    {
        return new ReportDto(report.Id, report.StartTime, report.EndTime, report.CreationTime, report.Author);
    }

    public static DetailedReportDto AsDetailedDto(this Report report)
    {
        return new DetailedReportDto(
            report.Id,
            report.StartTime,
            report.EndTime,
            report.CreationTime,
            report.Author,
            report.TotalHandled,
            report.SubordinateHandled,
            report.CountByAccount.Select(rs => rs.AsDto()).ToList());
    }

    public static ReportStatDto AsDto(this ReportStat stat)
    {
        return new ReportStatDto(stat.Account.AsDto(), stat.Count);
    }
}