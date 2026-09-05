using MiniShop.Application.Contracts;

namespace MiniShop.Application;

public interface IDashboardAppService
{
    Task<DashboardDto> GetAsync(CancellationToken cancellationToken);
}
