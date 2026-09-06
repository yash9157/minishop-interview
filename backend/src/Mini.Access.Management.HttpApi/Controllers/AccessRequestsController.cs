using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mini.Access.Management.Application.Contracts;
using Mini.Access.Management.Domain.Shared;

namespace Mini.Access.Management.HttpApi.Controllers;

[ApiController]
[Authorize]
[Route("api/access-requests")]
public sealed class AccessRequestsController(IAccessRequestAppService service) : ControllerBase
{
    [HttpGet("mine")]
    public Task<PagedResult<AccessRequestDto>> Mine(
        [FromQuery] PagedRequest page) =>
        service.GetMineAsync(User.GetUserId(), page);

    [HttpGet("pending-approvals")]
    public Task<PagedResult<AccessRequestDto>> Pending(
        [FromQuery] PagedRequest page) =>
        service.GetPendingAsync(User.GetUserId(), page);

    [Authorize(Roles = Roles.Admin + "," + Roles.Provisioner)]
    [HttpGet]
    public Task<PagedResult<AccessRequestDto>> All(
        [FromQuery] AccessRequestStatus? status, [FromQuery] PagedRequest page) =>
        service.GetAllAsync(status, page);

    [HttpPost]
    public async Task<ActionResult<AccessRequestDto>> Create(
        CreateAccessRequest request) =>
        StatusCode(StatusCodes.Status201Created,
            await service.CreateAsync(request, User.GetUserId()));

    [HttpPost("{id:long}/submit")]
    public Task<AccessRequestDto> Submit(long id) =>
        service.SubmitAsync(id, User.GetUserId());

    [HttpPost("{id:long}/approve")]
    public Task<AccessRequestDto> Approve(
        long id, ApprovalActionRequest request) =>
        service.ApproveAsync(id, User.GetUserId(), request.Remarks);

    [HttpPost("{id:long}/reject")]
    public Task<AccessRequestDto> Reject(
        long id, ApprovalActionRequest request) =>
        service.RejectAsync(id, User.GetUserId(), request.Remarks);

    [Authorize(Roles = Roles.Admin + "," + Roles.Provisioner)]
    [HttpPost("{id:long}/provision")]
    public Task<AccessRequestDto> Provision(long id) =>
        service.ProvisionAsync(id, User.GetUserId());
}
