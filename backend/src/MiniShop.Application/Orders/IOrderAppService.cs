using MiniShop.Application.Contracts;

namespace MiniShop.Application;

public interface IOrderAppService
{
    Task<PagedResult<OrderSummaryDto>> GetListAsync(OrderQuery request, CancellationToken cancellationToken);
    Task<OrderDetailsDto> GetAsync(long id, CancellationToken cancellationToken);
    Task<OrderDetailsDto> CreateAsync(SaveOrderRequest request, CancellationToken cancellationToken);
    Task<OrderDetailsDto> UpdateAsync(long id, SaveOrderRequest request, CancellationToken cancellationToken);
    Task DeleteAsync(long id, CancellationToken cancellationToken);
}
