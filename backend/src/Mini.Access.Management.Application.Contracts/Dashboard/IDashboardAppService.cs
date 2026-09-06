namespace Mini.Access.Management.Application.Contracts;

public interface IDashboardAppService
{
    Task<DashboardDto> GetAsync();
}
