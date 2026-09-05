using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniShop.Application;
using MiniShop.Application.Contracts;
using MiniShop.Domain.Shared;

namespace MiniShop.HttpApi.Controllers;

[ApiController]
[Authorize]
[Route("api/products")]
public sealed class ProductsController(IProductAppService productAppService) : ControllerBase
{
    [HttpGet]
    public Task<PagedResult<ProductDto>> GetList(
        [FromQuery] ProductQuery query,
        CancellationToken cancellationToken) =>
        productAppService.GetListAsync(query, cancellationToken);

    [HttpGet("{id:long}")]
    public Task<ProductDto> Get(long id, CancellationToken cancellationToken) =>
        productAppService.GetAsync(id, cancellationToken);

    [Authorize(Roles = Roles.Admin)]
    [HttpPost]
    public async Task<ActionResult<ProductDto>> Create(
        SaveProductRequest request,
        CancellationToken cancellationToken)
    {
        var result = await productAppService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = result.Id }, result);
    }

    [Authorize(Roles = Roles.Admin)]
    [HttpPut("{id:long}")]
    public Task<ProductDto> Update(
        long id,
        SaveProductRequest request,
        CancellationToken cancellationToken) =>
        productAppService.UpdateAsync(id, request, cancellationToken);

    [Authorize(Roles = Roles.Admin)]
    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id, CancellationToken cancellationToken)
    {
        await productAppService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
