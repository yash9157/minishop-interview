using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mini.Access.Management.Application.Contracts;
using Mini.Access.Management.Domain.Shared;

namespace Mini.Access.Management.HttpApi.Controllers;

[ApiController]
[Authorize(Roles = Roles.Admin)]
[Route("api/users")]
public sealed class UsersController(IUserAppService service) : ControllerBase
{
    [HttpGet]
    public Task<PagedResult<UserDto>> Get(
        [FromQuery] PagedRequest request) =>
        service.GetUsersAsync(request);

    [HttpPost]
    public async Task<ActionResult<UserDto>> Create(
        CreateUserRequest request,
        [FromHeader(Name = "Idempotency-Key")] string idempotencyKey) =>
        StatusCode(StatusCodes.Status201Created,
            await service.CreateUserAsync(
                request, idempotencyKey, User.GetUserId()));

    [HttpPut("{id:guid}")]
    public Task<UserDto> Update(
        Guid id, UpdateUserRequest request) =>
        service.UpdateUserAsync(id, request, User.GetUserId());

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await service.DeleteUserAsync(id, User.GetUserId());
        return NoContent();
    }

    [HttpPost("{id:guid}/roles")]
    public async Task<IActionResult> AssignRole(
        Guid id, AssignRoleRequest request)
    {
        await service.AssignRoleAsync(id, request.RoleId, User.GetUserId());
        return NoContent();
    }

    [HttpDelete("{id:guid}/roles/{roleId:guid}")]
    public async Task<IActionResult> RemoveRole(
        Guid id, Guid roleId)
    {
        await service.RemoveRoleAsync(id, roleId, User.GetUserId());
        return NoContent();
    }

    [HttpGet("{id:guid}/permissions")]
    public Task<string[]> Permissions(Guid id) =>
        service.GetEffectivePermissionsAsync(id);
}
