using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mini.Access.Management.Domain;

namespace Mini.Access.Management.EntityFrameworkCore;

public sealed class AccessRequestConfiguration : IEntityTypeConfiguration<AccessRequest>
{
    public void Configure(EntityTypeBuilder<AccessRequest> builder)
    {
        builder.Property(x => x.Version).IsConcurrencyToken();
        builder.HasIndex(x => new { x.RequesterId, x.CreatedAtUtc });
        builder.HasIndex(x => new { x.Status, x.CreatedAtUtc });
        builder.HasOne(x => x.Requester).WithMany().HasForeignKey(x => x.RequesterId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.RequestedRole).WithMany().HasForeignKey(x => x.RequestedRoleId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.TargetSystem).WithMany().HasForeignKey(x => x.TargetSystemId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.ProvisionedBy).WithMany().HasForeignKey(x => x.ProvisionedById)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
