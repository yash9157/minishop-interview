using System.Text.Json;
using Mini.Access.Management.Domain;
using Mini.Access.Management.EntityFrameworkCore;

namespace Mini.Access.Management.Application;

public sealed class AuditWriter(AccessManagementDbContext dbContext)
{
    public void Add(Guid? userId, string action, string entity, object entityId,
        object? oldValue = null, object? newValue = null) =>
        dbContext.AuditLogs.Add(new AuditLog
        {
            UserId = userId,
            Action = action,
            Entity = entity,
            EntityId = entityId.ToString()!,
            OldValue = oldValue is null ? null : JsonSerializer.Serialize(oldValue),
            NewValue = newValue is null ? null : JsonSerializer.Serialize(newValue)
        });
}
