using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mini.Access.Management.Domain;

namespace Mini.Access.Management.EntityFrameworkCore;

public sealed class ApprovalHistoryConfiguration : IEntityTypeConfiguration<ApprovalHistory>
{
    public void Configure(EntityTypeBuilder<ApprovalHistory> builder)
    {
        builder.HasIndex(x => new { x.ApproverId, x.Decision });
        builder.HasIndex(x => new { x.AccessRequestId, x.Level }).IsUnique();
        builder.HasOne(x => x.AccessRequest).WithMany(x => x.Approvals)
            .HasForeignKey(x => x.AccessRequestId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.Approver).WithMany().HasForeignKey(x => x.ApproverId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
