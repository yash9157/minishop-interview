using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MiniShop.Domain;

namespace MiniShop.EntityFrameworkCore;

public sealed class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.Property(x => x.Action).HasMaxLength(60).IsRequired();
        builder.Property(x => x.Entity).HasMaxLength(80).IsRequired();
        builder.Property(x => x.EntityId).HasMaxLength(80).IsRequired();
        builder.Property(x => x.OldValue).HasColumnType("longtext");
        builder.Property(x => x.NewValue).HasColumnType("longtext");
        builder.HasIndex(x => x.TimestampUtc);
        builder.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
