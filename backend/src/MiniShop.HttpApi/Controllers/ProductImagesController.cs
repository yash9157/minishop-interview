using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniShop.Application;
using MiniShop.Domain.Shared;

namespace MiniShop.HttpApi.Controllers;

[ApiController]
[Authorize]
[Route("api/products/{productId:long}/image")]
public sealed class ProductImagesController(IProductImageAppService imageAppService) : ControllerBase
{
    [HttpPost]
    [Authorize(Roles = Roles.Admin)]
    [RequestSizeLimit(6 * 1024 * 1024)]
    public async Task<IActionResult> Upload(
        long productId,
        [FromForm] IFormFile file,
        CancellationToken cancellationToken)
    {
        await imageAppService.UploadAsync(productId, file, cancellationToken);
        return NoContent();
    }

    [HttpGet]
    public async Task<IActionResult> Get(long productId, CancellationToken cancellationToken)
    {
        var image = await imageAppService.GetAsync(productId, cancellationToken);
        return PhysicalFile(image.FullPath, image.ContentType, enableRangeProcessing: true);
    }

    [HttpDelete]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Delete(long productId, CancellationToken cancellationToken)
    {
        await imageAppService.DeleteAsync(productId, cancellationToken);
        return NoContent();
    }
}
