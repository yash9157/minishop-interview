using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniShop.Application;
using MiniShop.Application.Contracts;
using MiniShop.Domain.Shared;

namespace MiniShop.HttpApi.Controllers;

[ApiController]
[Authorize]
[Route("api")]
public sealed class RolesController(UserRoleAppService service) : ControllerBase
{
    [HttpGet("roles")]
    public Task<RoleDto[]> Roles(CancellationToken cancellationToken) =>
        service.GetRolesAsync(cancellationToken);

    [HttpGet("permissions")]
    public Task<PermissionDto[]> Permissions(CancellationToken cancellationToken) =>
        service.GetPermissionsAsync(cancellationToken);

    [Authorize(Roles = MiniShop.Domain.Shared.Roles.Admin)]
    [HttpPost("permissions")]
    public async Task<ActionResult<PermissionDto>> CreatePermission(
        SavePermissionRequest request, CancellationToken cancellationToken) =>
        StatusCode(StatusCodes.Status201Created,
            await service.CreatePermissionAsync(request, User.GetUserId(), cancellationToken));

    [Authorize(Roles = MiniShop.Domain.Shared.Roles.Admin)]
    [HttpPut("permissions/{id:long}")]
    public Task<PermissionDto> UpdatePermission(
        long id, SavePermissionRequest request, CancellationToken cancellationToken) =>
        service.UpdatePermissionAsync(id, request, User.GetUserId(), cancellationToken);

    [Authorize(Roles = MiniShop.Domain.Shared.Roles.Admin)]
    [HttpDelete("permissions/{id:long}")]
    public async Task<IActionResult> DeletePermission(
        long id, CancellationToken cancellationToken)
    {
        await service.DeletePermissionAsync(id, User.GetUserId(), cancellationToken);
        return NoContent();
    }

    [HttpGet("target-systems")]
    public Task<TargetSystemDto[]> Systems(CancellationToken cancellationToken) =>
        service.GetSystemsAsync(cancellationToken);

    [Authorize(Roles = MiniShop.Domain.Shared.Roles.Admin)]
    [HttpPost("roles")]
    public async Task<ActionResult<RoleDto>> Create(
        SaveRoleRequest request, CancellationToken cancellationToken) =>
        StatusCode(StatusCodes.Status201Created,
            await service.CreateRoleAsync(request, User.GetUserId(), cancellationToken));

    [Authorize(Roles = MiniShop.Domain.Shared.Roles.Admin)]
    [HttpPut("roles/{id:guid}")]
    public Task<RoleDto> Update(
        Guid id, SaveRoleRequest request, CancellationToken cancellationToken) =>
        service.UpdateRoleAsync(id, request, User.GetUserId(), cancellationToken);

    [Authorize(Roles = MiniShop.Domain.Shared.Roles.Admin)]
    [HttpDelete("roles/{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await service.DeleteRoleAsync(id, User.GetUserId(), cancellationToken);
        return NoContent();
    }

    [Authorize(Roles = MiniShop.Domain.Shared.Roles.Admin)]
    [HttpPut("roles/{id:guid}/permissions")]
    public async Task<IActionResult> SetPermissions(
        Guid id, SetRolePermissionsRequest request, CancellationToken cancellationToken)
    {
        await service.SetPermissionsAsync(
            id, request.PermissionIds, User.GetUserId(), cancellationToken);
        return NoContent();
    }
}
