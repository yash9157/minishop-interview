using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MiniShop.Domain;
using MiniShop.Domain.Shared;

namespace MiniShop.EntityFrameworkCore;

public sealed class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.Property(user => user.FullName)
            .HasMaxLength(ValidationConstants.NameMaxLength)
            .IsRequired();
        builder.HasQueryFilter(user => !user.IsDeleted);
        builder.HasOne(user => user.Manager)
            .WithMany(user => user.DirectReports)
            .HasForeignKey(user => user.ManagerId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(user => new { user.IsDeleted, user.IsActive });
    }
}
