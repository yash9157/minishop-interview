using System.ComponentModel.DataAnnotations;

namespace Mini.Access.Management.Domain;

public sealed class IdempotencyRecord
{
    public long Id { get; set; }

    [MaxLength(100)]
    public string Key { get; set; } = string.Empty;

    [MaxLength(80)]
    public string Operation { get; set; } = string.Empty;
    public string RequestHash { get; set; } = string.Empty;
    public string ResourceId { get; set; } = string.Empty;
    public string ResponseJson { get; set; } = string.Empty;
    public int StatusCode { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
