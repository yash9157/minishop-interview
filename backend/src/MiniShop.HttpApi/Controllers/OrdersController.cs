using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniShop.Application;
using MiniShop.Application.Contracts;
using MiniShop.Domain.Shared;

namespace MiniShop.HttpApi.Controllers;

[ApiController]
[Authorize(Roles = Roles.Admin)]
[Route("api/orders")]
public sealed class OrdersController(IOrderAppService orderAppService) : ControllerBase
{
    [HttpGet]
    public Task<PagedResult<OrderSummaryDto>> GetList(
        [FromQuery] OrderQuery query,
        CancellationToken cancellationToken) =>
        orderAppService.GetListAsync(query, cancellationToken);

    [HttpGet("{id:long}")]
    public Task<OrderDetailsDto> Get(long id, CancellationToken cancellationToken) =>
        orderAppService.GetAsync(id, cancellationToken);

    [HttpPost]
    public async Task<ActionResult<OrderDetailsDto>> Create(
        SaveOrderRequest request,
        CancellationToken cancellationToken)
    {
        var result = await orderAppService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = result.Id }, result);
    }

    [HttpPut("{id:long}")]
    public Task<OrderDetailsDto> Update(
        long id,
        SaveOrderRequest request,
        CancellationToken cancellationToken) =>
        orderAppService.UpdateAsync(id, request, cancellationToken);

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id, CancellationToken cancellationToken)
    {
        await orderAppService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
