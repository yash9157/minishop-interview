using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mini.Access.Management.Domain;

namespace Mini.Access.Management.EntityFrameworkCore;

public sealed class TargetSystemConfiguration : IEntityTypeConfiguration<TargetSystem>
{
    public void Configure(EntityTypeBuilder<TargetSystem> builder)
    {
        builder.HasIndex(x => x.Name).IsUnique();
    }
}
