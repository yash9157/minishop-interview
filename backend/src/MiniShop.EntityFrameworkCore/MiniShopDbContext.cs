using System.Reflection;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MiniShop.Domain;
using MiniShop.Domain.Shared;

namespace MiniShop.EntityFrameworkCore;

public sealed class MiniShopDbContext(
    DbContextOptions<MiniShopDbContext> options,
    IHttpContextAccessor httpContextAccessor)
    : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>(options)
{
    private long? CurrentTenantId
    {
        get
        {
            var value = httpContextAccessor.HttpContext?.User.FindFirstValue("tenant_id");
            return long.TryParse(value, out var tenantId) ? tenantId : null;
        }
    }

    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(MiniShopDbContext).Assembly);
        ApplyMultiTenantFilters(builder);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var tenantId = CurrentTenantId;
        foreach (var entry in ChangeTracker.Entries<IMultiTenant>())
        {
            if (entry.State == EntityState.Added)
            {
                if (tenantId.HasValue)
                    entry.Entity.TenantId = tenantId.Value;
                else if (entry.Entity.TenantId <= 0)
                    throw new InvalidOperationException("TenantId is required.");
            }

            if (entry.State is EntityState.Modified or EntityState.Deleted &&
                tenantId.HasValue && entry.Entity.TenantId != tenantId.Value)
            {
                throw new InvalidOperationException("Cross-tenant write is not allowed.");
            }

            if (entry.State == EntityState.Modified)
                entry.Property(entity => entity.TenantId).IsModified = false;
        }

        return base.SaveChangesAsync(cancellationToken);
    }

    private void ApplyMultiTenantFilters(ModelBuilder builder)
    {
        var entityTypes = builder.Model.GetEntityTypes()
            .Where(entityType =>
                entityType.BaseType is null &&
                !entityType.IsOwned() &&
                typeof(IMultiTenant).IsAssignableFrom(entityType.ClrType));

        var method = typeof(MiniShopDbContext).GetMethod(
            nameof(SetMultiTenantFilter),
            BindingFlags.Instance | BindingFlags.NonPublic)!;

        foreach (var entityType in entityTypes)
            method.MakeGenericMethod(entityType.ClrType).Invoke(this, [builder]);
    }

    private void SetMultiTenantFilter<TEntity>(ModelBuilder builder)
        where TEntity : class, IMultiTenant =>
        builder.Entity<TEntity>().HasQueryFilter(entity =>
            CurrentTenantId.HasValue && entity.TenantId == CurrentTenantId.Value);
}
