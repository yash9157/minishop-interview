using Microsoft.AspNetCore.Identity;

namespace MiniShop.Domain;

public sealed class ApplicationUser : IdentityUser<Guid>
{
    public string FullName { get; set; } = string.Empty;
    public Guid? ManagerId { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? DeletedAtUtc { get; set; }

    public ApplicationUser? Manager { get; set; }
    public ICollection<ApplicationUser> DirectReports { get; set; } = [];
}
