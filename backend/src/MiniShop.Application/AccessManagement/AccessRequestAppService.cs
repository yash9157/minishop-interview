using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MiniShop.Application.Contracts;
using MiniShop.Domain;
using MiniShop.Domain.Shared;
using MiniShop.EntityFrameworkCore;

namespace MiniShop.Application;

public sealed class AccessRequestAppService(
    MiniShopDbContext db,
    UserManager<ApplicationUser> users,
    AuditWriter audit) : IAccessRequestAppService
{
    public Task<PagedResult<AccessRequestDto>> GetMineAsync(
        Guid userId, PagedRequest page, CancellationToken cancellationToken) =>
        GetPageAsync(Query().Where(x => x.RequesterId == userId), page, cancellationToken);

    public Task<PagedResult<AccessRequestDto>> GetPendingAsync(
        Guid approverId, PagedRequest page, CancellationToken cancellationToken) =>
        GetPageAsync(Query()
            .Where(x => x.Status == AccessRequestStatus.Pending &&
                x.Approvals.Any(a => a.ApproverId == approverId &&
                    a.Decision == ApprovalDecision.Pending &&
                    !x.Approvals.Any(previous =>
                        previous.Level < a.Level && previous.Decision != ApprovalDecision.Approved))),
            page, cancellationToken);

    public Task<PagedResult<AccessRequestDto>> GetAllAsync(
        AccessRequestStatus? status, PagedRequest page, CancellationToken cancellationToken)
    {
        var query = Query();
        if (status.HasValue)
            query = query.Where(x => x.Status == status);
        return GetPageAsync(query, page, cancellationToken);
    }

    public async Task<AccessRequestDto> CreateAsync(
        CreateAccessRequest request, Guid userId, CancellationToken cancellationToken)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        if (!await db.TargetSystems.AnyAsync(
                x => x.Id == request.TargetSystemId && x.IsActive, cancellationToken))
            throw new BusinessException("Target system does not exist.");
        var role = await db.Roles.FirstOrDefaultAsync(
            x => x.Id == request.RequestedRoleId && x.IsRequestable, cancellationToken)
            ?? throw new BusinessException("Requested role does not exist.");

        var entity = new AccessRequest
        {
            RequesterId = userId,
            TargetSystemId = request.TargetSystemId,
            RequestedRoleId = role.Id,
            BusinessJustification = request.BusinessJustification.Trim()
        };
        db.AccessRequests.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        audit.Add(userId, "Create", "AccessRequest", entity.Id, newValue: entity.Status);
        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return await GetAsync(entity.Id, cancellationToken);
    }

    public async Task<AccessRequestDto> SubmitAsync(
        long id, Guid userId, CancellationToken cancellationToken)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        var request = await db.AccessRequests
            .Include(x => x.Requester)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new NotFoundException("Access request was not found.");
        if (request.RequesterId != userId)
            throw new UnauthorizedException("You can submit only your own request.");
        if (request.Status != AccessRequestStatus.Draft)
            throw new BusinessException("Only draft requests can be submitted.");
        if (!request.Requester.ManagerId.HasValue)
            throw new BusinessException("A manager must be assigned before submitting.");

        var securityUsers = await users.GetUsersInRoleAsync(Roles.SecurityAdmin);
        var securityApprover = securityUsers.FirstOrDefault(x =>
            x.IsActive && !x.IsDeleted && x.Id != request.Requester.ManagerId)
            ?? securityUsers.FirstOrDefault(x => x.IsActive && !x.IsDeleted)
            ?? throw new BusinessException("No active Security/Admin approver is configured.");

        request.Status = AccessRequestStatus.Pending;
        request.SubmittedAtUtc = DateTime.UtcNow;
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
        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return await GetAsync(id, cancellationToken);
    }

    public Task<AccessRequestDto> ApproveAsync(
        long id, Guid approverId, string? remarks, CancellationToken cancellationToken) =>
        DecideAsync(id, approverId, ApprovalDecision.Approved, remarks, cancellationToken);

    public Task<AccessRequestDto> RejectAsync(
        long id, Guid approverId, string? remarks, CancellationToken cancellationToken) =>
        DecideAsync(id, approverId, ApprovalDecision.Rejected, remarks, cancellationToken);

    public async Task<AccessRequestDto> ProvisionAsync(
        long id, Guid actorId, CancellationToken cancellationToken)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        var request = await db.AccessRequests
            .Include(x => x.Requester)
            .Include(x => x.RequestedRole)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new NotFoundException("Access request was not found.");
        if (request.Status != AccessRequestStatus.Approved)
            throw new BusinessException("Only approved requests can be provisioned.");
        if (!request.Requester.IsActive || request.Requester.IsDeleted)
            throw new BusinessException("The requester is inactive.");
        if (await users.IsInRoleAsync(request.Requester, request.RequestedRole.Name!))
            throw new ConflictException("The requested role is already assigned.");
        if ((request.RequestedRole.Name == Roles.Maker &&
                await users.IsInRoleAsync(request.Requester, Roles.Checker)) ||
            (request.RequestedRole.Name == Roles.Checker &&
                await users.IsInRoleAsync(request.Requester, Roles.Maker)))
            throw new ConflictException("Maker and Checker roles cannot be assigned together.");

        var result = await users.AddToRoleAsync(request.Requester, request.RequestedRole.Name!);
        if (!result.Succeeded)
            throw new BusinessException(string.Join(" ", result.Errors.Select(x => x.Description)));
        request.Status = AccessRequestStatus.Provisioned;
        request.ProvisionedById = actorId;
        request.ProvisionedAtUtc = DateTime.UtcNow;
        audit.Add(actorId, "Provision", "AccessRequest", id,
            AccessRequestStatus.Approved, AccessRequestStatus.Provisioned);
        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return await GetAsync(id, cancellationToken);
    }

    private async Task<AccessRequestDto> DecideAsync(
        long id, Guid approverId, ApprovalDecision decision, string? remarks,
        CancellationToken cancellationToken)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        var request = await db.AccessRequests.Include(x => x.Approvals)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new NotFoundException("Access request was not found.");
        if (request.Status != AccessRequestStatus.Pending)
            throw new BusinessException("This request is not pending.");

        var next = request.Approvals.Where(x => x.Decision == ApprovalDecision.Pending)
            .OrderBy(x => x.Level).FirstOrDefault()
            ?? throw new BusinessException("No pending approval exists.");
        if (next.ApproverId != approverId)
            throw new UnauthorizedException("This request is not awaiting your approval.");

        next.Decision = decision;
        next.Remarks = remarks?.Trim();
        next.DecisionAtUtc = DateTime.UtcNow;
        if (decision == ApprovalDecision.Rejected)
            request.Status = AccessRequestStatus.Rejected;
        else if (request.Approvals.All(x =>
                     x.Id == next.Id || x.Decision == ApprovalDecision.Approved))
            request.Status = AccessRequestStatus.Approved;
        audit.Add(approverId, decision.ToString(), "AccessRequest", id,
            newValue: new { next.Level, next.Remarks });
        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return await GetAsync(id, cancellationToken);
    }

    private async Task<AccessRequestDto> GetAsync(long id, CancellationToken cancellationToken) =>
        await Query().Where(x => x.Id == id).Select(MapExpression())
            .FirstAsync(cancellationToken);

    private IQueryable<AccessRequest> Query() => db.AccessRequests.AsNoTracking();

    private static async Task<PagedResult<AccessRequestDto>> GetPageAsync(
        IQueryable<AccessRequest> query, PagedRequest page, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(page.Search))
        {
            var search = page.Search.Trim();
            query = query.Where(x => x.Requester.FullName.Contains(search) ||
                x.TargetSystem.Name.Contains(search) || x.RequestedRole.Name!.Contains(search));
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query.OrderByDescending(x => x.CreatedAtUtc)
            .Skip((page.Page - 1) * page.PageSize)
            .Take(page.PageSize)
            .Select(MapExpression())
            .ToArrayAsync(cancellationToken);
        return new PagedResult<AccessRequestDto>(items, totalCount, page.Page, page.PageSize);
    }

    private static System.Linq.Expressions.Expression<Func<AccessRequest, AccessRequestDto>>
        MapExpression() => x => new AccessRequestDto(
            x.Id, x.RequesterId, x.Requester.FullName, x.TargetSystemId, x.TargetSystem.Name,
            x.RequestedRoleId, x.RequestedRole.Name!, x.BusinessJustification, x.Status,
            x.CreatedAtUtc, x.Approvals.OrderBy(a => a.Level)
                .Select(a => new ApprovalDto(a.Id, a.Level, a.ApproverId,
                    a.Approver.FullName, a.Decision, a.Remarks, a.DecisionAtUtc)).ToArray());
}
