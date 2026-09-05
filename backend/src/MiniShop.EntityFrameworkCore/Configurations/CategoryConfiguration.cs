using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MiniShop.Domain;
using MiniShop.Domain.Shared;

namespace MiniShop.EntityFrameworkCore;

public sealed class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("Categories");
        builder.HasKey(category => category.Id);
        builder.Property(category => category.Name)
            .HasMaxLength(ValidationConstants.NameMaxLength)
            .IsRequired();
        builder.Property(category => category.Description)
            .HasMaxLength(ValidationConstants.DescriptionMaxLength);
        builder.HasIndex(category => new { category.TenantId, category.Name }).IsUnique();
        builder.HasOne(category => category.Tenant)
            .WithMany(tenant => tenant.Categories)
            .HasForeignKey(category => category.TenantId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
