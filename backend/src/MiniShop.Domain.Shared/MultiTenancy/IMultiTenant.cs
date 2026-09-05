namespace MiniShop.Domain.Shared;

public interface IMultiTenant
{
    long TenantId { get; set; }
}
