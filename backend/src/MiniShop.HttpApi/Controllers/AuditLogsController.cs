using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniShop.Application;
using MiniShop.Application.Contracts;
using MiniShop.Domain.Shared;

namespace MiniShop.HttpApi.Controllers;

[ApiController]
[Authorize(Roles = Roles.Admin)]
[Route("api/audit-logs")]
public sealed class AuditLogsController(IAuditLogAppService service) : ControllerBase
{
    [HttpGet]
    public Task<PagedResult<AuditLogDto>> Get(
        [FromQuery] PagedRequest page, CancellationToken cancellationToken) =>
        service.GetAsync(page, cancellationToken);
}
