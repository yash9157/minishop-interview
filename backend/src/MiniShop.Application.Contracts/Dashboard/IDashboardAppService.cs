namespace MiniShop.Application.Contracts;

public interface IDashboardAppService
{
    Task<DashboardDto> GetAsync(CancellationToken cancellationToken);
}
