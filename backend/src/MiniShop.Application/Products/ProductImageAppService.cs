using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MiniShop.Domain;
using MiniShop.EntityFrameworkCore;

namespace MiniShop.Application;

public sealed class ProductImageAppService(
    MiniShopDbContext dbContext,
    IHostEnvironment environment,
    ILogger<ProductImageAppService> logger) : IProductImageAppService
{
    private const long MaxFileSize = 5 * 1024 * 1024;

    private static readonly IReadOnlyDictionary<string, string> AllowedTypes =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            [".jpg"] = "image/jpeg",
            [".jpeg"] = "image/jpeg",
            [".png"] = "image/png",
            [".webp"] = "image/webp"
        };

    private readonly string uploadsRoot = Path.GetFullPath(
        Path.Combine(environment.ContentRootPath, "uploads"));

    public async Task UploadAsync(
        long productId,
        IFormFile file,
        CancellationToken cancellationToken)
    {
        if (file.Length == 0)
            throw new BusinessException("Select an image to upload.");
        if (file.Length > MaxFileSize)
            throw new BusinessException("The image must be 5 MB or smaller.");

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedTypes.TryGetValue(extension, out var contentType) ||
            (!string.Equals(file.ContentType, contentType, StringComparison.OrdinalIgnoreCase) &&
             !string.Equals(file.ContentType, "application/octet-stream", StringComparison.OrdinalIgnoreCase)))
            throw new BusinessException("Only JPG, PNG, and WebP images are allowed.");

        var product = await FindProductAsync(productId, cancellationToken);
        var relativePath = $"products/{Guid.NewGuid():N}{extension}";
        var fullPath = ResolvePath(relativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
        var oldPath = product.ImagePath;

        try
        {
            await using var stream = new FileStream(
                fullPath,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None,
                bufferSize: 81920,
                useAsync: true);
            await file.CopyToAsync(stream, cancellationToken);

            product.ImagePath = relativePath;
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            if (File.Exists(fullPath))
                File.Delete(fullPath);
            throw;
        }

        if (oldPath is not null)
            DeleteFile(oldPath);
    }

    public async Task<ProductImageResult> GetAsync(
        long productId,
        CancellationToken cancellationToken)
    {
        var product = await dbContext.Products
            .AsNoTracking()
            .SingleOrDefaultAsync(product => product.Id == productId, cancellationToken)
            ?? throw new NotFoundException("Product was not found.");

        if (product.ImagePath is null)
            throw new NotFoundException("Product image was not found.");

        var fullPath = ResolvePath(product.ImagePath);
        if (!File.Exists(fullPath))
            throw new NotFoundException("Product image was not found.");

        var extension = Path.GetExtension(fullPath);
        if (!AllowedTypes.TryGetValue(extension, out var contentType))
            throw new NotFoundException("Product image was not found.");

        return new ProductImageResult(fullPath, contentType);
    }

    public async Task DeleteAsync(long productId, CancellationToken cancellationToken)
    {
        var product = await FindProductAsync(productId, cancellationToken);
        if (product.ImagePath is null)
            return;

        var oldPath = product.ImagePath;
        product.ImagePath = null;
        await dbContext.SaveChangesAsync(cancellationToken);
        DeleteFile(oldPath);
    }

    private async Task<Product> FindProductAsync(
        long productId,
        CancellationToken cancellationToken) =>
        await dbContext.Products.SingleOrDefaultAsync(
            product => product.Id == productId,
            cancellationToken)
        ?? throw new NotFoundException("Product was not found.");

    private string ResolvePath(string relativePath)
    {
        var fullPath = Path.GetFullPath(Path.Combine(
            uploadsRoot,
            relativePath.Replace('/', Path.DirectorySeparatorChar)));
        var requiredPrefix = uploadsRoot.TrimEnd(Path.DirectorySeparatorChar) +
            Path.DirectorySeparatorChar;

        if (!fullPath.StartsWith(requiredPrefix, StringComparison.OrdinalIgnoreCase))
            throw new BusinessException("The stored image path is invalid.");

        return fullPath;
    }

    private void DeleteFile(string relativePath)
    {
        try
        {
            var fullPath = ResolvePath(relativePath);
            if (File.Exists(fullPath))
                File.Delete(fullPath);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            logger.LogWarning(exception, "Could not delete product image {ImagePath}.", relativePath);
        }
    }
}
