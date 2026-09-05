using Microsoft.EntityFrameworkCore;
using MiniShop.Application.Contracts;
using MiniShop.Domain;
using MiniShop.EntityFrameworkCore;

namespace MiniShop.Application;

public sealed class ProductAppService(MiniShopDbContext dbContext) : IProductAppService
{
    public async Task<PagedResult<ProductDto>> GetListAsync(
        ProductQuery request,
        CancellationToken cancellationToken)
    {
        var query = dbContext.Products.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim();
            query = query.Where(product =>
                product.Name.Contains(search) || product.Sku.Contains(search));
        }

        if (request.CategoryId.HasValue)
            query = query.Where(product => product.CategoryId == request.CategoryId);
        if (request.IsActive.HasValue)
            query = query.Where(product => product.IsActive == request.IsActive);

        query = (request.SortBy.ToLowerInvariant(), request.Descending) switch
        {
            ("price", false) => query.OrderBy(product => product.Price),
            ("price", true) => query.OrderByDescending(product => product.Price),
            ("sku", false) => query.OrderBy(product => product.Sku),
            ("sku", true) => query.OrderByDescending(product => product.Sku),
            (_, true) => query.OrderByDescending(product => product.Name),
            _ => query.OrderBy(product => product.Name)
        };

        var totalCount = await query.CountAsync(cancellationToken);
        var products = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(product => new ProductDto(
                product.Id,
                product.CategoryId,
                product.Category.Name,
                product.Sku,
                product.Name,
                product.Price,
                product.StockQuantity,
                product.IsActive))
            .ToListAsync(cancellationToken);

        return new PagedResult<ProductDto>(
            products,
            totalCount,
            request.Page,
            request.PageSize);
    }

    public async Task<ProductDto> GetAsync(long id, CancellationToken cancellationToken) =>
        Map(await FindAsync(id, cancellationToken));

    public async Task<ProductDto> CreateAsync(
        SaveProductRequest request,
        CancellationToken cancellationToken)
    {
        var category = await FindCategoryAsync(request.CategoryId, cancellationToken);
        var sku = NormalizeSku(request.Sku);
        await EnsureSkuIsUniqueAsync(sku, null, cancellationToken);

        var product = new Product
        {
            CategoryId = category.Id,
            Category = category,
            Sku = sku,
            Name = request.Name.Trim(),
            Price = request.Price,
            StockQuantity = request.StockQuantity,
            IsActive = request.IsActive
        };

        dbContext.Add(product);
        await dbContext.SaveChangesAsync(cancellationToken);
        return Map(product);
    }

    public async Task<ProductDto> UpdateAsync(
        long id,
        SaveProductRequest request,
        CancellationToken cancellationToken)
    {
        var product = await FindAsync(id, cancellationToken);
        var category = await FindCategoryAsync(request.CategoryId, cancellationToken);
        var sku = NormalizeSku(request.Sku);
        await EnsureSkuIsUniqueAsync(sku, id, cancellationToken);

        product.CategoryId = category.Id;
        product.Category = category;
        product.Sku = sku;
        product.Name = request.Name.Trim();
        product.Price = request.Price;
        product.StockQuantity = request.StockQuantity;
        product.IsActive = request.IsActive;

        await dbContext.SaveChangesAsync(cancellationToken);
        return Map(product);
    }

    public async Task DeleteAsync(long id, CancellationToken cancellationToken)
    {
        var product = await FindAsync(id, cancellationToken);
        if (await dbContext.OrderItems.AnyAsync(
                item => item.ProductId == id,
                cancellationToken))
            throw new ConflictException("Product is used by an order.");

        dbContext.Remove(product);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task<Product> FindAsync(long id, CancellationToken cancellationToken) =>
        await dbContext.Products
            .Include(product => product.Category)
            .FirstOrDefaultAsync(product => product.Id == id, cancellationToken)
        ?? throw new NotFoundException("Product was not found.");

    private async Task<Category> FindCategoryAsync(long id, CancellationToken cancellationToken) =>
        await dbContext.Categories.FirstOrDefaultAsync(
            category => category.Id == id,
            cancellationToken)
        ?? throw new BusinessException("Category does not exist.");

    private async Task EnsureSkuIsUniqueAsync(
        string sku,
        long? excludingId,
        CancellationToken cancellationToken)
    {
        var exists = await dbContext.Products.AnyAsync(
            product => product.Sku == sku && (!excludingId.HasValue || product.Id != excludingId),
            cancellationToken);
        if (exists)
            throw new ConflictException("SKU already exists.");
    }

    private static string NormalizeSku(string sku) => sku.Trim().ToUpperInvariant();

    private static ProductDto Map(Product product) =>
        new(
            product.Id,
            product.CategoryId,
            product.Category?.Name ?? string.Empty,
            product.Sku,
            product.Name,
            product.Price,
            product.StockQuantity,
            product.IsActive);
}
