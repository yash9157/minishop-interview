using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MiniShop.Domain;

namespace MiniShop.EntityFrameworkCore;

public sealed class ApplicationRoleConfiguration : IEntityTypeConfiguration<ApplicationRole>
{
    public void Configure(EntityTypeBuilder<ApplicationRole> builder) =>
        builder.Property(role => role.IsRequestable).HasDefaultValue(false);
}

public sealed class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        builder.Property(x => x.Code).HasMaxLength(80).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(120).IsRequired();
        builder.HasIndex(x => x.Code).IsUnique();
    }
}

public sealed class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
{
    public void Configure(EntityTypeBuilder<RolePermission> builder)
    {
        builder.HasKey(x => new { x.RoleId, x.PermissionId });
        builder.HasOne(x => x.Role).WithMany().HasForeignKey(x => x.RoleId);
        builder.HasOne(x => x.Permission).WithMany(x => x.Roles).HasForeignKey(x => x.PermissionId);
    }
}

public sealed class TargetSystemConfiguration : IEntityTypeConfiguration<TargetSystem>
{
    public void Configure(EntityTypeBuilder<TargetSystem> builder)
    {
        builder.Property(x => x.Name).HasMaxLength(120).IsRequired();
        builder.HasIndex(x => x.Name).IsUnique();
    }
}

public sealed class AccessRequestConfiguration : IEntityTypeConfiguration<AccessRequest>
{
    public void Configure(EntityTypeBuilder<AccessRequest> builder)
    {
        builder.Property(x => x.BusinessJustification).HasMaxLength(1000).IsRequired();
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
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

public sealed class ApprovalHistoryConfiguration : IEntityTypeConfiguration<ApprovalHistory>
{
    public void Configure(EntityTypeBuilder<ApprovalHistory> builder)
    {
        builder.Property(x => x.Decision).HasConversion<string>().HasMaxLength(20);
        builder.Property(x => x.Remarks).HasMaxLength(500);
        builder.HasIndex(x => new { x.ApproverId, x.Decision });
        builder.HasIndex(x => new { x.AccessRequestId, x.Level }).IsUnique();
        builder.HasOne(x => x.AccessRequest).WithMany(x => x.Approvals)
            .HasForeignKey(x => x.AccessRequestId);
        builder.HasOne(x => x.Approver).WithMany().HasForeignKey(x => x.ApproverId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

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
