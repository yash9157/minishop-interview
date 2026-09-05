using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniShop.Application;
using MiniShop.Application.Contracts;
using MiniShop.Domain.Shared;

namespace MiniShop.HttpApi.Controllers;

[ApiController]
[Authorize(Roles = Roles.Admin)]
[Route("api/customers")]
public sealed class CustomersController(ICustomerAppService customerAppService) : ControllerBase
{
    [HttpGet]
    public Task<PagedResult<CustomerDto>> GetList(
        [FromQuery] PagedRequest query,
        CancellationToken cancellationToken) =>
        customerAppService.GetListAsync(query, cancellationToken);

    [HttpGet("{id:long}")]
    public Task<CustomerDto> Get(long id, CancellationToken cancellationToken) =>
        customerAppService.GetAsync(id, cancellationToken);

    [HttpPost]
    public async Task<ActionResult<CustomerDto>> Create(
        SaveCustomerRequest request,
        CancellationToken cancellationToken)
    {
        var result = await customerAppService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = result.Id }, result);
    }

    [HttpPut("{id:long}")]
    public Task<CustomerDto> Update(
        long id,
        SaveCustomerRequest request,
        CancellationToken cancellationToken) =>
        customerAppService.UpdateAsync(id, request, cancellationToken);

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id, CancellationToken cancellationToken)
    {
        await customerAppService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
