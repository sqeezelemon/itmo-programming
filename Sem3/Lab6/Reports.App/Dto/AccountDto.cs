using Reports.Models;

namespace Reports.App.Dto;

public record AccountDto(Guid id, string name, string serviceId, string ownerLogin);