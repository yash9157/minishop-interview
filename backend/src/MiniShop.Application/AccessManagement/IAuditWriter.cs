namespace MiniShop.Application;

public interface IAuditWriter
{
    void Add(Guid? userId, string action, string entity, object entityId,
        object? oldValue = null, object? newValue = null);
}
