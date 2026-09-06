using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Mini.Access.Management.Application.Contracts;
using Mini.Access.Management.Domain;
using Mini.Access.Management.Domain.Shared;
using Mini.Access.Management.EntityFrameworkCore;

namespace Mini.Access.Management.Application;

public sealed class AccessRequestAppService(
    AccessManagementDbContext db,
    UserManager<ApplicationUser> users,
    AuditWriter audit) : IAccessRequestAppService
{
    public Task<PagedResult<AccessRequestDto>> GetMineAsync(
        Guid userId, PagedRequest page) =>
        GetPageAsync(Query().Where(x => x.RequesterId == userId), page);

    public Task<PagedResult<AccessRequestDto>> GetPendingAsync(
        Guid approverId, PagedRequest page) =>
        GetPageAsync(Query()
          .Where(x => x.Status == AccessRequestStatus.Pending &&
              x.Approvals.Any(a => a.ApproverId == approverId &&
                  a.Decision == ApprovalDecision.Pending &&
                !x.Approvals.Any(previous =>
                    previous.Level < a.Level && previous.Decision != ApprovalDecision.Approved))),
            page);

    public Task<PagedResult<AccessRequestDto>> GetAllAsync(
        AccessRequestStatus? status, PagedRequest page)
    {
        var query = Query();
        if (status.HasValue)
            query = query.Where(x => x.Status == status);
        return GetPageAsync(query, page);
    }

    public async Task<AccessRequestDto> CreateAsync(
        CreateAccessRequest request, Guid userId)
    {
        await using var transaction = await db.Database.BeginTransactionAsync();
        if (!await db.TargetSystems.AnyAsync(
                x => x.Id == request.TargetSystemId && x.IsActive))
            throw new ArgumentException("Target system does not exist.");
        var role = await db.Roles.FirstOrDefaultAsync(
            x => x.Id == request.RequestedRoleId && x.IsRequestable)
            ?? throw new ArgumentException("Requested role does not exist.");

        var entity = new AccessRequest
        {
            RequesterId = userId,
            TargetSystemId = request.TargetSystemId,
            RequestedRoleId = role.Id,
            BusinessJustification = request.BusinessJustification.Trim()
        };
        db.AccessRequests.Add(entity);
        await db.SaveChangesAsync();
        audit.Add(userId, "Create", "AccessRequest", entity.Id, newValue: entity.Status);
        await db.SaveChangesAsync();
        await transaction.CommitAsync();
        return await GetAsync(entity.Id);
    }

    public async Task<AccessRequestDto> SubmitAsync(
        long id, Guid userId)
    {
        await using var transaction = await db.Database.BeginTransactionAsync();
        var request = await db.AccessRequests
            .Include(x => x.Requester)
            .FirstOrDefaultAsync(x => x.Id == id)
            ?? throw new KeyNotFoundException("Access request was not found.");
        if (request.RequesterId != userId)
            throw new System.Security.SecurityException("You can submit only your own request.");
        if (request.Status != AccessRequestStatus.Draft)
            throw new ArgumentException("Only draft requests can be submitted.");
        if (!request.Requester.ManagerId.HasValue)
            throw new ArgumentException("A manager must be assigned before submitting.");

        var securityUsers = await users.GetUsersInRoleAsync(Roles.SecurityAdmin);
        var securityApprover = securityUsers.FirstOrDefault(x =>
            x.IsActive && !x.IsDeleted && x.Id != request.Requester.ManagerId)
            ?? securityUsers.FirstOrDefault(x => x.IsActive && !x.IsDeleted)
            ?? throw new ArgumentException("No active Security/Admin approver is configured.");

        request.Status = AccessRequestStatus.Pending;
        request.SubmittedAtUtc = DateTime.UtcNow;
        request.Version++;
        db.ApprovalHistory.AddRange(
            new ApprovalHistory
            {
                AccessRequestId = id,
                Level = 1,
                ApproverId = request.Requester.ManagerId.Value
            },
            new ApprovalHistory
            {
                AccessRequestId = id,
                Level = 2,
                ApproverId = securityApprover.Id
            });
        audit.Add(userId, "Submit", "AccessRequest", id, AccessRequestStatus.Draft,
            AccessRequestStatus.Pending);
        await db.SaveChangesAsync();
        await transaction.CommitAsync();
        return await GetAsync(id);
    }

    public Task<AccessRequestDto> ApproveAsync(
        long id, Guid approverId, string remarks) =>
        DecideAsync(id, approverId, ApprovalDecision.Approved, remarks);

    public Task<AccessRequestDto> RejectAsync(
        long id, Guid approverId, string remarks) =>
        DecideAsync(id, approverId, ApprovalDecision.Rejected, remarks);

    public async Task<AccessRequestDto> ProvisionAsync(
        long id, Guid actorId)
    {
        await using var transaction = await db.Database.BeginTransactionAsync();
        var request = await db.AccessRequests
            .Include(x => x.Requester)
            .Include(x => x.RequestedRole)
            .FirstOrDefaultAsync(x => x.Id == id)
            ?? throw new KeyNotFoundException("Access request was not found.");
        if (request.Status != AccessRequestStatus.Approved)
            throw new ArgumentException("Only approved requests can be provisioned.");
        if (!request.Requester.IsActive || request.Requester.IsDeleted)
            throw new ArgumentException("The requester is inactive.");
        if (await users.IsInRoleAsync(request.Requester, request.RequestedRole.Name!))
            throw new InvalidOperationException("The requested role is already assigned.");
        if ((request.RequestedRole.Name == Roles.Maker &&
                await users.IsInRoleAsync(request.Requester, Roles.Checker)) ||
            (request.RequestedRole.Name == Roles.Checker &&
                await users.IsInRoleAsync(request.Requester, Roles.Maker)))
            throw new InvalidOperationException("Maker and Checker roles cannot be assigned together.");

        var result = await users.AddToRoleAsync(request.Requester, request.RequestedRole.Name!);
        if (!result.Succeeded)
            throw new ArgumentException(string.Join(" ", result.Errors.Select(x => x.Description)));
        request.Status = AccessRequestStatus.Provisioned;
        request.ProvisionedById = actorId;
        request.ProvisionedAtUtc = DateTime.UtcNow;
        request.Version++;
        audit.Add(actorId, "Provision", "AccessRequest", id,
            AccessRequestStatus.Approved, AccessRequestStatus.Provisioned);
        await db.SaveChangesAsync();
        await transaction.CommitAsync();
        return await GetAsync(id);
    }

    private async Task<AccessRequestDto> DecideAsync(
        long id, Guid approverId, ApprovalDecision decision, string remarks)
    {
        await using var transaction = await db.Database.BeginTransactionAsync();
        var request = await db.AccessRequests.Include(x => x.Approvals)
            .FirstOrDefaultAsync(x => x.Id == id)
            ?? throw new KeyNotFoundException("Access request was not found.");
        if (request.Status != AccessRequestStatus.Pending)
            throw new ArgumentException("This request is not pending.");

        var next = request.Approvals.Where(x => x.Decision == ApprovalDecision.Pending)
            .OrderBy(x => x.Level).FirstOrDefault()
            ?? throw new ArgumentException("No pending approval exists.");
        if (next.ApproverId != approverId)
            throw new System.Security.SecurityException("This request is not awaiting your approval.");

        next.Decision = decision;
        next.Remarks = remarks.Trim();
        next.DecisionAtUtc = DateTime.UtcNow;
        if (decision == ApprovalDecision.Rejected)
            request.Status = AccessRequestStatus.Rejected;
        else if (request.Approvals.All(x =>
                     x.Id == next.Id || x.Decision == ApprovalDecision.Approved))
            request.Status = AccessRequestStatus.Approved;
        request.Version++;
        audit.Add(approverId, decision.ToString(), "AccessRequest", id,
            newValue: new { next.Level, next.Remarks });
        await db.SaveChangesAsync();
        await transaction.CommitAsync();
        return await GetAsync(id);
    }

    private async Task<AccessRequestDto> GetAsync(long id) =>
        await Query().Where(x => x.Id == id).Select(MapExpression())
            .FirstAsync();

    private IQueryable<AccessRequest> Query() => db.AccessRequests.AsNoTracking();

    private static async Task<PagedResult<AccessRequestDto>> GetPageAsync(
        IQueryable<AccessRequest> query, PagedRequest page)
    {
        if (!string.IsNullOrWhiteSpace(page.Search))
        {
            var search = page.Search.Trim();
            query = query.Where(x => x.Requester.FullName.Contains(search) ||
                x.TargetSystem.Name.Contains(search) || x.RequestedRole.Name!.Contains(search));
        }

        var totalCount = await query.CountAsync();
        var items = await query.OrderByDescending(x => x.CreatedAtUtc)
            .Skip((page.Page - 1) * page.PageSize)
            .Take(page.PageSize)
            .Select(MapExpression())
            .ToArrayAsync();
        return new PagedResult<AccessRequestDto>(items, totalCount, page.Page, page.PageSize);
    }

    private static System.Linq.Expressions.Expression<Func<AccessRequest, AccessRequestDto>>
        MapExpression() => x => new AccessRequestDto(
            x.Id, x.RequesterId, x.Requester.FullName, x.TargetSystemId, x.TargetSystem.Name,
            x.RequestedRoleId, x.RequestedRole.Name!, x.BusinessJustification, x.Status,
            x.CreatedAtUtc, x.SubmittedAtUtc, x.ProvisionedById,
            x.ProvisionedBy == null ? null : x.ProvisionedBy.FullName, x.ProvisionedAtUtc,
            x.Approvals.OrderBy(a => a.Level)
                .Select(a => new ApprovalDto(a.Id, a.Level, a.ApproverId,
                    a.Approver.FullName, a.Decision, a.Remarks, a.DecisionAtUtc)).ToArray());
}
