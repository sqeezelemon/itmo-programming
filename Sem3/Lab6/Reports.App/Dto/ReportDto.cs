using Reports.Models;

namespace Reports.App.Dto;

public record ReportDto(Guid id,
    DateTime startTime,
    DateTime endTime,
    DateTime creationTime,
    Employee author);