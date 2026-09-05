using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniShop.Application;
using MiniShop.Application.Contracts;
using MiniShop.Domain.Shared;

namespace MiniShop.HttpApi.Controllers;

[ApiController]
[Authorize]
[Route("api/access-requests")]
public sealed class AccessRequestsController(AccessRequestAppService service) : ControllerBase
{
    [HttpGet("mine")]
    public Task<AccessRequestDto[]> Mine(CancellationToken cancellationToken) =>
        service.GetMineAsync(User.GetUserId(), cancellationToken);

    [HttpGet("pending-approvals")]
    public Task<AccessRequestDto[]> Pending(CancellationToken cancellationToken) =>
        service.GetPendingAsync(User.GetUserId(), cancellationToken);

    [Authorize(Roles = Roles.Admin + "," + Roles.Provisioner)]
    [HttpGet]
    public Task<AccessRequestDto[]> All(
        [FromQuery] AccessRequestStatus? status, CancellationToken cancellationToken) =>
        service.GetAllAsync(status, cancellationToken);

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
