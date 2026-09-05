using MiniShop.Application.Contracts;

namespace MiniShop.Application;

public interface IProductAppService
{
    Task<PagedResult<ProductDto>> GetListAsync(ProductQuery request, CancellationToken cancellationToken);
    Task<ProductDto> GetAsync(long id, CancellationToken cancellationToken);
    Task<ProductDto> CreateAsync(SaveProductRequest request, CancellationToken cancellationToken);
    Task<ProductDto> UpdateAsync(long id, SaveProductRequest request, CancellationToken cancellationToken);
    Task DeleteAsync(long id, CancellationToken cancellationToken);
}
