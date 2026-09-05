using Microsoft.EntityFrameworkCore;
using MiniShop.Application.Contracts;
using MiniShop.Domain;
using MiniShop.EntityFrameworkCore;

namespace MiniShop.Application;

public sealed class CategoryAppService(MiniShopDbContext dbContext) : ICategoryAppService
{
    public async Task<IReadOnlyList<CategoryDto>> GetAllAsync(CancellationToken cancellationToken) =>
        await dbContext.Categories
            .AsNoTracking()
            .OrderBy(category => category.Name)
            .Select(category => new CategoryDto(category.Id, category.Name, category.Description))
            .ToListAsync(cancellationToken);

    public async Task<CategoryDto> GetAsync(long id, CancellationToken cancellationToken) =>
        Map(await FindAsync(id, cancellationToken));

    public async Task<CategoryDto> CreateAsync(
        SaveCategoryRequest request,
        CancellationToken cancellationToken)
    {
        var name = request.Name.Trim();
        if (await NameExistsAsync(name, null, cancellationToken))
            throw new ConflictException("Category name already exists.");

        var category = new Category
        {
            Name = name,
            Description = request.Description?.Trim()
        };
        dbContext.Add(category);
        await dbContext.SaveChangesAsync(cancellationToken);
        return Map(category);
    }

    public async Task<CategoryDto> UpdateAsync(
        long id,
        SaveCategoryRequest request,
        CancellationToken cancellationToken)
    {
        var category = await FindAsync(id, cancellationToken);
        var name = request.Name.Trim();
        if (await NameExistsAsync(name, id, cancellationToken))
            throw new ConflictException("Category name already exists.");

        category.Name = name;
        category.Description = request.Description?.Trim();
        await dbContext.SaveChangesAsync(cancellationToken);
        return Map(category);
    }

    public async Task DeleteAsync(long id, CancellationToken cancellationToken)
    {
        var category = await FindAsync(id, cancellationToken);
        if (await dbContext.Products.AnyAsync(
                product => product.CategoryId == id,
                cancellationToken))
            throw new ConflictException("Category is used by products.");

        dbContext.Remove(category);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task<Category> FindAsync(long id, CancellationToken cancellationToken) =>
        await dbContext.Categories.FirstOrDefaultAsync(
            category => category.Id == id,
            cancellationToken)
        ?? throw new NotFoundException("Category was not found.");

    private Task<bool> NameExistsAsync(
        string name,
        long? excludingId,
        CancellationToken cancellationToken) =>
        dbContext.Categories.AnyAsync(
            category => category.Name == name && (!excludingId.HasValue || category.Id != excludingId),
            cancellationToken);

    private static CategoryDto Map(Category category) =>
        new(category.Id, category.Name, category.Description);
}
