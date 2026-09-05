using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MiniShop.Domain;
using MiniShop.Domain.Shared;

namespace MiniShop.EntityFrameworkCore;

public sealed class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("Customers");
        builder.HasKey(customer => customer.Id);
        builder.Property(customer => customer.Name)
            .HasMaxLength(ValidationConstants.NameMaxLength)
            .IsRequired();
        builder.Property(customer => customer.Email)
            .HasMaxLength(ValidationConstants.EmailMaxLength)
            .IsRequired();
        builder.Property(customer => customer.Phone)
            .HasMaxLength(ValidationConstants.PhoneMaxLength);
        builder.HasIndex(customer => new { customer.TenantId, customer.Email }).IsUnique();
        builder.HasOne(customer => customer.Tenant)
            .WithMany(tenant => tenant.Customers)
            .HasForeignKey(customer => customer.TenantId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
