using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MiniShop.Domain;

namespace MiniShop.EntityFrameworkCore;

public sealed class ApprovalHistoryConfiguration : IEntityTypeConfiguration<ApprovalHistory>
{
    public void Configure(EntityTypeBuilder<ApprovalHistory> builder)
    {
        builder.Property(x => x.Decision).HasConversion<string>().HasMaxLength(20);
        builder.Property(x => x.Remarks).HasMaxLength(500);
        builder.HasIndex(x => new { x.ApproverId, x.Decision });
        builder.HasIndex(x => new { x.AccessRequestId, x.Level }).IsUnique();
        builder.HasOne(x => x.AccessRequest).WithMany(x => x.Approvals)
            .HasForeignKey(x => x.AccessRequestId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.Approver).WithMany().HasForeignKey(x => x.ApproverId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
