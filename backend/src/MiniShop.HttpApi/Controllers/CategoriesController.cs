using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniShop.Application;
using MiniShop.Application.Contracts;
using MiniShop.Domain.Shared;

namespace MiniShop.HttpApi.Controllers;

[ApiController]
[Authorize]
[Route("api/categories")]
public sealed class CategoriesController(ICategoryAppService categoryAppService) : ControllerBase
{
    [HttpGet]
    public Task<IReadOnlyList<CategoryDto>> GetAll(CancellationToken cancellationToken) =>
        categoryAppService.GetAllAsync(cancellationToken);

    [HttpGet("{id:long}")]
    public Task<CategoryDto> Get(long id, CancellationToken cancellationToken) =>
        categoryAppService.GetAsync(id, cancellationToken);

    [Authorize(Roles = Roles.Admin)]
    [HttpPost]
    public async Task<ActionResult<CategoryDto>> Create(
        SaveCategoryRequest request,
        CancellationToken cancellationToken)
    {
        var result = await categoryAppService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = result.Id }, result);
    }

    [Authorize(Roles = Roles.Admin)]
    [HttpPut("{id:long}")]
    public Task<CategoryDto> Update(
        long id,
        SaveCategoryRequest request,
        CancellationToken cancellationToken) =>
        categoryAppService.UpdateAsync(id, request, cancellationToken);

    [Authorize(Roles = Roles.Admin)]
    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id, CancellationToken cancellationToken)
    {
        await categoryAppService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
