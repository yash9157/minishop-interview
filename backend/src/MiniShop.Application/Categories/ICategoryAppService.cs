using MiniShop.Application.Contracts;

namespace MiniShop.Application;

public interface ICategoryAppService
{
    Task<IReadOnlyList<CategoryDto>> GetAllAsync(CancellationToken cancellationToken);
    Task<CategoryDto> GetAsync(long id, CancellationToken cancellationToken);
    Task<CategoryDto> CreateAsync(SaveCategoryRequest request, CancellationToken cancellationToken);
    Task<CategoryDto> UpdateAsync(long id, SaveCategoryRequest request, CancellationToken cancellationToken);
    Task DeleteAsync(long id, CancellationToken cancellationToken);
}
