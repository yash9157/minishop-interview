using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MiniShop.Domain;

namespace MiniShop.EntityFrameworkCore;

public sealed class TargetSystemConfiguration : IEntityTypeConfiguration<TargetSystem>
{
    public void Configure(EntityTypeBuilder<TargetSystem> builder)
    {
        builder.Property(x => x.Name).HasMaxLength(120).IsRequired();
        builder.HasIndex(x => x.Name).IsUnique();
    }
}
