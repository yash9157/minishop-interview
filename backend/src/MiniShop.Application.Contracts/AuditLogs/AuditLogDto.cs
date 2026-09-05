namespace MiniShop.Application.Contracts;

public sealed record AuditLogDto(
    long Id, string User, string Action, string Entity, string EntityId,
    DateTime TimestampUtc, string? OldValue, string? NewValue);
