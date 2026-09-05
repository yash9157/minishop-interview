namespace MiniShop.Domain;

public sealed class AuditLog
{
    public long Id { get; set; }
    public Guid? UserId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string Entity { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public ApplicationUser? User { get; set; }
}
