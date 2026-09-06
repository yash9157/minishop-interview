using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mini.Access.Management.Application.Contracts;
using Mini.Access.Management.Domain.Shared;

namespace Mini.Access.Management.HttpApi.Controllers;

[ApiController]
[Authorize]
[Route("api")]
public sealed class RolesController(IRolePermissionAppService service) : ControllerBase
{
    [HttpGet("roles")]
    public Task<PagedResult<RoleDto>> Roles(
        [FromQuery] PagedRequest request) =>
        service.GetRolesAsync(request);

    [HttpGet("permissions")]
    public Task<PagedResult<PermissionDto>> Permissions(
        [FromQuery] PagedRequest request) =>
        service.GetPermissionsAsync(request);

    [Authorize(Roles = Mini.Access.Management.Domain.Shared.Roles.Admin)]
    [HttpPost("permissions")]
    public async Task<ActionResult<PermissionDto>> CreatePermission(
        SavePermissionRequest request) =>
        StatusCode(StatusCodes.Status201Created,
            await service.CreatePermissionAsync(request, User.GetUserId()));

    [Authorize(Roles = Mini.Access.Management.Domain.Shared.Roles.Admin)]
    [HttpPut("permissions/{id:long}")]
    public Task<PermissionDto> UpdatePermission(
        long id, SavePermissionRequest request) =>
        service.UpdatePermissionAsync(id, request, User.GetUserId());

    [Authorize(Roles = Mini.Access.Management.Domain.Shared.Roles.Admin)]
    [HttpDelete("permissions/{id:long}")]
    public async Task<IActionResult> DeletePermission(
        long id)
    {
        await service.DeletePermissionAsync(id, User.GetUserId());
        return NoContent();
    }

    [HttpGet("target-systems")]
    public Task<TargetSystemDto[]> Systems() =>
        service.GetSystemsAsync();

    [Authorize(Roles = Mini.Access.Management.Domain.Shared.Roles.Admin)]
    [HttpPost("roles")]
    public async Task<ActionResult<RoleDto>> Create(
        SaveRoleRequest request) =>
        StatusCode(StatusCodes.Status201Created,
            await service.CreateRoleAsync(request, User.GetUserId()));

    [Authorize(Roles = Mini.Access.Management.Domain.Shared.Roles.Admin)]
    [HttpPut("roles/{id:guid}")]
    public Task<RoleDto> Update(
        Guid id, SaveRoleRequest request) =>
        service.UpdateRoleAsync(id, request, User.GetUserId());

    [Authorize(Roles = Mini.Access.Management.Domain.Shared.Roles.Admin)]
    [HttpDelete("roles/{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await service.DeleteRoleAsync(id, User.GetUserId());
        return NoContent();
    }

    [Authorize(Roles = Mini.Access.Management.Domain.Shared.Roles.Admin)]
    [HttpPut("roles/{id:guid}/permissions")]
    public async Task<IActionResult> SetPermissions(
        Guid id, SetRolePermissionsRequest request)
    {
        await service.SetPermissionsAsync(
            id, request.PermissionIds, User.GetUserId());
        return NoContent();
    }
}
