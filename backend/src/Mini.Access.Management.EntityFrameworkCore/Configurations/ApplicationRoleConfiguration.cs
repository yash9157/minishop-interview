using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mini.Access.Management.Domain;

namespace Mini.Access.Management.EntityFrameworkCore;

public sealed class ApplicationRoleConfiguration : IEntityTypeConfiguration<ApplicationRole>
{
    public void Configure(EntityTypeBuilder<ApplicationRole> builder) =>
        builder.Property(role => role.IsRequestable).HasDefaultValue(false);
}
