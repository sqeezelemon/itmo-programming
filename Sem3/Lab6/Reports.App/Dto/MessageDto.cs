using Reports.Data.Enums;

namespace Reports.App.Dto;

public record MessageDto(
    Guid id,
    string serviceId,
    string senderId,
    string recepientId,
    string value,
    DateTime timestamp,
    MessageState state);