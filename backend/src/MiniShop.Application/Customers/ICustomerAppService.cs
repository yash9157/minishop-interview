using MiniShop.Application.Contracts;

namespace MiniShop.Application;

public interface ICustomerAppService
{
    Task<PagedResult<CustomerDto>> GetListAsync(PagedRequest request, CancellationToken cancellationToken);
    Task<CustomerDto> GetAsync(long id, CancellationToken cancellationToken);
    Task<CustomerDto> CreateAsync(SaveCustomerRequest request, CancellationToken cancellationToken);
    Task<CustomerDto> UpdateAsync(long id, SaveCustomerRequest request, CancellationToken cancellationToken);
    Task DeleteAsync(long id, CancellationToken cancellationToken);
}
