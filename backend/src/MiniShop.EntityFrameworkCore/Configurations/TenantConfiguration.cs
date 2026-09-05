using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MiniShop.Domain;
using MiniShop.Domain.Shared;

namespace MiniShop.EntityFrameworkCore;

public sealed class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.ToTable("Tenants");
        builder.HasKey(tenant => tenant.Id);
        builder.Property(tenant => tenant.Code)
            .HasMaxLength(ValidationConstants.NameMaxLength)
            .IsRequired();
        builder.Property(tenant => tenant.Name)
            .HasMaxLength(ValidationConstants.NameMaxLength)
            .IsRequired();
        builder.HasIndex(tenant => tenant.Code).IsUnique();
    }
}
