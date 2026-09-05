using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MiniShop.Domain;
using MiniShop.Domain.Shared;

namespace MiniShop.EntityFrameworkCore;

public sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");
        builder.HasKey(product => product.Id);
        builder.Property(product => product.Sku)
            .HasMaxLength(ValidationConstants.SkuMaxLength)
            .IsRequired();
        builder.Property(product => product.Name)
            .HasMaxLength(ValidationConstants.NameMaxLength)
            .IsRequired();
        builder.Property(product => product.Price).HasPrecision(18, 2);
        builder.Property(product => product.ImagePath).HasMaxLength(300);
        builder.HasIndex(product => new { product.TenantId, product.Sku }).IsUnique();
        builder.HasIndex(product => new { product.TenantId, product.CategoryId, product.Name });
        builder.HasOne(product => product.Tenant)
            .WithMany(tenant => tenant.Products)
            .HasForeignKey(product => product.TenantId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(product => product.Category)
            .WithMany(category => category.Products)
            .HasForeignKey(product => product.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
