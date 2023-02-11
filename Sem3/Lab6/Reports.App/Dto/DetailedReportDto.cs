using Reports.Models;

namespace Reports.App.Dto;

public record DetailedReportDto(Guid id,
    DateTime startTime,
    DateTime endTime,
    DateTime creationTime,
    Employee author,
    int totalHandled,
    int subordinateHandled,
    IReadOnlyList<ReportStatDto> countByAccount);