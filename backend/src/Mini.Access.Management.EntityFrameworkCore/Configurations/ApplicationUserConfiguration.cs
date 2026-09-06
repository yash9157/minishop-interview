using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mini.Access.Management.Domain;

namespace Mini.Access.Management.EntityFrameworkCore;

public sealed class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.HasOne(user => user.Manager)
            .WithMany(user => user.DirectReports)
            .HasForeignKey(user => user.ManagerId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(user => new { user.IsDeleted, user.IsActive });
    }
}
