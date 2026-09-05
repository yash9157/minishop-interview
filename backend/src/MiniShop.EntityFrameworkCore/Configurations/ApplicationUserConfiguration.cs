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
        builder.HasOne(user => user.Tenant)
            .WithMany(tenant => tenant.Users)
            .HasForeignKey(user => user.TenantId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
