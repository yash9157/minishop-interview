using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MiniShop.Domain;

namespace MiniShop.EntityFrameworkCore;

public sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders");
        builder.HasKey(order => order.Id);
        builder.Property(order => order.OrderNumber).HasMaxLength(40).IsRequired();
        builder.Property(order => order.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(order => order.TotalAmount).HasPrecision(18, 2);
        builder.HasIndex(order => order.OrderNumber).IsUnique();
        builder.HasIndex(order => new { order.CustomerId, order.OrderDateUtc });
        builder.HasIndex(order => new { order.Status, order.OrderDateUtc });
        builder.HasOne(order => order.Customer)
            .WithMany(customer => customer.Orders)
            .HasForeignKey(order => order.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
