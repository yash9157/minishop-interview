namespace MiniShop.Domain.Shared;

public enum AccessRequestStatus
{
    Draft,
    Pending,
    Approved,
    Rejected,
    Provisioned
}

public enum ApprovalDecision
{
    Pending,
    Approved,
    Rejected
}
