using Microsoft.AspNetCore.Http;

namespace MiniShop.Application;

public interface IProductImageAppService
{
    Task UploadAsync(long productId, IFormFile file, CancellationToken cancellationToken);
    Task<ProductImageResult> GetAsync(long productId, CancellationToken cancellationToken);
    Task DeleteAsync(long productId, CancellationToken cancellationToken);
}

public sealed record ProductImageResult(string FullPath, string ContentType);
