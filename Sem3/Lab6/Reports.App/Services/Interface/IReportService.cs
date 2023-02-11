using Reports.App.Dto;

namespace Reports.App.Services;

public interface IReportService
{
    Task<IReadOnlyList<ReportDto>> ListReports();
    Task<IReadOnlyList<ReportDto>> ListReports(string supervisorLogin);
    Task<IReadOnlyList<ReportDto>> ListReports(DateTime startTime, DateTime endTime);
    Task<DetailedReportDto> GetDetailedReport(Guid id);
    Task<DetailedReportDto> MakeReport(DateTime startTime, DateTime endTime, string supervisorLogin);
}