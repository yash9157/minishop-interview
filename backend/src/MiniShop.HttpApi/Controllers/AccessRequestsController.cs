using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniShop.Application;
using MiniShop.Application.Contracts;
using MiniShop.Domain.Shared;

namespace MiniShop.HttpApi.Controllers;

[ApiController]
[Authorize]
[Route("api/access-requests")]
public sealed class AccessRequestsController(IAccessRequestAppService service) : ControllerBase
{
    [HttpGet("mine")]
    public Task<PagedResult<AccessRequestDto>> Mine(
        [FromQuery] PagedRequest page, CancellationToken cancellationToken) =>
        service.GetMineAsync(User.GetUserId(), page, cancellationToken);

    [HttpGet("pending-approvals")]
    public Task<PagedResult<AccessRequestDto>> Pending(
        [FromQuery] PagedRequest page, CancellationToken cancellationToken) =>
        service.GetPendingAsync(User.GetUserId(), page, cancellationToken);

    [Authorize(Roles = Roles.Admin + "," + Roles.Provisioner)]
    [HttpGet]
    public Task<PagedResult<AccessRequestDto>> All(
        [FromQuery] AccessRequestStatus? status, [FromQuery] PagedRequest page,
        CancellationToken cancellationToken) =>
        service.GetAllAsync(status, page, cancellationToken);

    [HttpPost]
    public async Task<ActionResult<AccessRequestDto>> Create(
        CreateAccessRequest request, CancellationToken cancellationToken) =>
        StatusCode(StatusCodes.Status201Created,
            await service.CreateAsync(request, User.GetUserId(), cancellationToken));

    [HttpPost("{id:long}/submit")]
    public Task<AccessRequestDto> Submit(long id, CancellationToken cancellationToken) =>
        service.SubmitAsync(id, User.GetUserId(), cancellationToken);

    [HttpPost("{id:long}/approve")]
    public Task<AccessRequestDto> Approve(
        long id, ApprovalActionRequest request, CancellationToken cancellationToken) =>
        service.ApproveAsync(id, User.GetUserId(), request.Remarks, cancellationToken);

    [HttpPost("{id:long}/reject")]
    public Task<AccessRequestDto> Reject(
        long id, ApprovalActionRequest request, CancellationToken cancellationToken) =>
        service.RejectAsync(id, User.GetUserId(), request.Remarks, cancellationToken);

    [Authorize(Roles = Roles.Admin + "," + Roles.Provisioner)]
    [HttpPost("{id:long}/provision")]
    public Task<AccessRequestDto> Provision(long id, CancellationToken cancellationToken) =>
        service.ProvisionAsync(id, User.GetUserId(), cancellationToken);
}
