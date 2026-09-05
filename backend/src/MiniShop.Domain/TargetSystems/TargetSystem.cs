namespace MiniShop.Domain;

public sealed class TargetSystem
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}
