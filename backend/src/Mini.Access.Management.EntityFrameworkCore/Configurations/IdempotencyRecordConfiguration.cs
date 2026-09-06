using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mini.Access.Management.Domain;

namespace Mini.Access.Management.EntityFrameworkCore;

public sealed class IdempotencyRecordConfiguration : IEntityTypeConfiguration<IdempotencyRecord>
{
    public void Configure(EntityTypeBuilder<IdempotencyRecord> builder)
    {
        builder.HasIndex(x => new { x.Operation, x.Key }).IsUnique();
    }
}
