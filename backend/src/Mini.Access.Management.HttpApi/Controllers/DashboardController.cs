using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mini.Access.Management.Application.Contracts;
using Mini.Access.Management.Domain.Shared;

namespace Mini.Access.Management.HttpApi.Controllers;

[ApiController]
[Authorize(Roles = Roles.Admin)]
[Route("api/dashboard")]
public sealed class DashboardController(IDashboardAppService service) : ControllerBase
{
    [HttpGet]
    public Task<DashboardDto> Get() =>
        service.GetAsync();
}
