using MiniShop.Application.Contracts;
using MiniShop.Domain.Shared;

namespace MiniShop.Application;

public interface IAccessRequestAppService
{
    Task<PagedResult<AccessRequestDto>> GetMineAsync(Guid userId, PagedRequest page, CancellationToken cancellationToken);
    Task<PagedResult<AccessRequestDto>> GetPendingAsync(Guid approverId, PagedRequest page, CancellationToken cancellationToken);
    Task<PagedResult<AccessRequestDto>> GetAllAsync(AccessRequestStatus? status, PagedRequest page, CancellationToken cancellationToken);
    Task<AccessRequestDto> CreateAsync(CreateAccessRequest request, Guid userId, CancellationToken cancellationToken);
    Task<AccessRequestDto> SubmitAsync(long id, Guid userId, CancellationToken cancellationToken);
    Task<AccessRequestDto> ApproveAsync(long id, Guid approverId, string? remarks, CancellationToken cancellationToken);
    Task<AccessRequestDto> RejectAsync(long id, Guid approverId, string? remarks, CancellationToken cancellationToken);
    Task<AccessRequestDto> ProvisionAsync(long id, Guid actorId, CancellationToken cancellationToken);
}
