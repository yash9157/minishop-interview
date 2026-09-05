using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MiniShop.Domain;

namespace MiniShop.EntityFrameworkCore;

public sealed class IdempotencyRecordConfiguration : IEntityTypeConfiguration<IdempotencyRecord>
{
    public void Configure(EntityTypeBuilder<IdempotencyRecord> builder)
    {
        builder.Property(x => x.Key).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Operation).HasMaxLength(80).IsRequired();
        builder.Property(x => x.ResourceId).HasMaxLength(80).IsRequired();
        builder.HasIndex(x => new { x.Operation, x.Key }).IsUnique();
    }
}
