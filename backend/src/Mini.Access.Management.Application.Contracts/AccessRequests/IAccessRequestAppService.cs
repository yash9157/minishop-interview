using Mini.Access.Management.Domain.Shared;

namespace Mini.Access.Management.Application.Contracts;

public interface IAccessRequestAppService
{
    Task<PagedResult<AccessRequestDto>> GetMineAsync(Guid userId, PagedRequest page);
    Task<PagedResult<AccessRequestDto>> GetPendingAsync(Guid approverId, PagedRequest page);
    Task<PagedResult<AccessRequestDto>> GetAllAsync(AccessRequestStatus? status, PagedRequest page);
    Task<AccessRequestDto> CreateAsync(CreateAccessRequest request, Guid userId);
    Task<AccessRequestDto> SubmitAsync(long id, Guid userId);
    Task<AccessRequestDto> ApproveAsync(long id, Guid approverId, string remarks);
    Task<AccessRequestDto> RejectAsync(long id, Guid approverId, string remarks);
    Task<AccessRequestDto> ProvisionAsync(long id, Guid actorId);
}
