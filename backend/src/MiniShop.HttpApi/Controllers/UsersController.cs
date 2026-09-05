using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniShop.Application;
using MiniShop.Application.Contracts;
using MiniShop.Domain.Shared;

namespace MiniShop.HttpApi.Controllers;

[ApiController]
[Authorize(Roles = Roles.Admin)]
[Route("api/users")]
public sealed class UsersController(IUserRoleAppService service) : ControllerBase
{
    [HttpGet]
    public Task<PagedResult<UserDto>> Get(
        [FromQuery] PagedRequest request, CancellationToken cancellationToken) =>
        service.GetUsersAsync(request, cancellationToken);

    [HttpPost]
    public async Task<ActionResult<UserDto>> Create(
        CreateUserRequest request,
        [FromHeader(Name = "Idempotency-Key")] string idempotencyKey,
        CancellationToken cancellationToken) =>
        StatusCode(StatusCodes.Status201Created,
            await service.CreateUserAsync(
                request, idempotencyKey, User.GetUserId(), cancellationToken));

    [HttpPut("{id:guid}")]
    public Task<UserDto> Update(
        Guid id, UpdateUserRequest request, CancellationToken cancellationToken) =>
        service.UpdateUserAsync(id, request, User.GetUserId(), cancellationToken);

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await service.DeleteUserAsync(id, User.GetUserId(), cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/roles")]
    public async Task<IActionResult> AssignRole(
        Guid id, AssignRoleRequest request, CancellationToken cancellationToken)
    {
        await service.AssignRoleAsync(id, request.RoleId, User.GetUserId(), cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:guid}/roles/{roleId:guid}")]
    public async Task<IActionResult> RemoveRole(
        Guid id, Guid roleId, CancellationToken cancellationToken)
    {
        await service.RemoveRoleAsync(id, roleId, User.GetUserId(), cancellationToken);
        return NoContent();
    }

    [HttpGet("{id:guid}/permissions")]
    public Task<string[]> Permissions(Guid id, CancellationToken cancellationToken) =>
        service.GetEffectivePermissionsAsync(id, cancellationToken);
}
