using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mini.Access.Management.Application.Contracts;
using Mini.Access.Management.Domain.Shared;

namespace Mini.Access.Management.HttpApi.Controllers;

[ApiController]
[Authorize(Roles = Roles.Admin)]
[Route("api/audit-logs")]
public sealed class AuditLogsController(IAuditLogAppService service) : ControllerBase
{
    [HttpGet]
    public Task<PagedResult<AuditLogDto>> Get(
        [FromQuery] PagedRequest page) =>
        service.GetAsync(page);
}
