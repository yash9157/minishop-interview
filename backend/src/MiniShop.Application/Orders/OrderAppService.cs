using Microsoft.EntityFrameworkCore;
using MiniShop.Application.Contracts;
using MiniShop.Domain;
using MiniShop.EntityFrameworkCore;

namespace MiniShop.Application;

public sealed class OrderAppService(MiniShopDbContext dbContext) : IOrderAppService
{
    public async Task<PagedResult<OrderSummaryDto>> GetListAsync(
        OrderQuery request,
        CancellationToken cancellationToken)
    {
        var query = dbContext.Orders.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim();
            query = query.Where(order =>
                order.OrderNumber.Contains(search) || order.Customer.Name.Contains(search));
        }

        if (request.CustomerId.HasValue)
            query = query.Where(order => order.CustomerId == request.CustomerId);
        if (request.Status.HasValue)
            query = query.Where(order => order.Status == request.Status);
        if (request.FromUtc.HasValue)
            query = query.Where(order => order.OrderDateUtc >= request.FromUtc.Value);
        if (request.ToUtc.HasValue)
            query = query.Where(order => order.OrderDateUtc <= request.ToUtc.Value);

        query = request.Descending
            ? query.OrderByDescending(order => order.OrderDateUtc)
            : query.OrderBy(order => order.OrderDateUtc);

        var totalCount = await query.CountAsync(cancellationToken);
        var orders = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(order => new OrderSummaryDto(
                order.Id,
                order.OrderNumber,
                order.CustomerId,
                order.Customer.Name,
                order.OrderDateUtc,
                order.Status,
                order.TotalAmount,
                order.Items.Count))
            .ToListAsync(cancellationToken);

        return new PagedResult<OrderSummaryDto>(
            orders,
            totalCount,
            request.Page,
            request.PageSize);
    }

    public async Task<OrderDetailsDto> GetAsync(long id, CancellationToken cancellationToken) =>
        Map(await FindOrderAsync(id, false, cancellationToken));

    public Task<OrderDetailsDto> CreateAsync(
        SaveOrderRequest request,
        CancellationToken cancellationToken) =>
        ExecuteInTransactionAsync(async () =>
        {
            var customer = await FindCustomerAsync(request.CustomerId, cancellationToken);
            var products = await ValidateAndLoadProductsAsync(request.Items, cancellationToken);
            var order = new Order
            {
                CustomerId = customer.Id,
                Customer = customer,
                OrderNumber = CreateOrderNumber(),
                OrderDateUtc = request.OrderDateUtc?.ToUniversalTime() ?? DateTime.UtcNow,
                Status = request.Status
            };

            ReplaceItems(order, request.Items, products);
            dbContext.Add(order);
            await dbContext.SaveChangesAsync(cancellationToken);
            return Map(order);
        }, cancellationToken);

    public Task<OrderDetailsDto> UpdateAsync(
        long id,
        SaveOrderRequest request,
        CancellationToken cancellationToken) =>
        ExecuteInTransactionAsync(async () =>
        {
            var order = await FindOrderAsync(id, true, cancellationToken);
            var customer = await FindCustomerAsync(request.CustomerId, cancellationToken);
            var products = await ValidateAndLoadProductsAsync(request.Items, cancellationToken);

            order.CustomerId = customer.Id;
            order.Customer = customer;
            order.OrderDateUtc = request.OrderDateUtc?.ToUniversalTime() ?? order.OrderDateUtc;
            order.Status = request.Status;
            ReplaceItems(order, request.Items, products);

            await dbContext.SaveChangesAsync(cancellationToken);
            return Map(order);
        }, cancellationToken);

    public async Task DeleteAsync(long id, CancellationToken cancellationToken)
    {
        var order = await FindOrderAsync(id, true, cancellationToken);
        dbContext.Remove(order);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task<Order> FindOrderAsync(
        long id,
        bool tracking,
        CancellationToken cancellationToken)
    {
        IQueryable<Order> query = dbContext.Orders;
        if (!tracking)
            query = query.AsNoTracking();

        return await query
            .Include(order => order.Customer)
            .Include(order => order.Items)
                .ThenInclude(item => item.Product)
            .FirstOrDefaultAsync(order => order.Id == id, cancellationToken)
            ?? throw new NotFoundException("Order was not found.");
    }

    private async Task<Customer> FindCustomerAsync(long id, CancellationToken cancellationToken) =>
        await dbContext.Customers.FirstOrDefaultAsync(
            customer => customer.Id == id,
            cancellationToken)
        ?? throw new BusinessException("Customer does not exist.");

    private async Task<Dictionary<long, Product>> ValidateAndLoadProductsAsync(
        IReadOnlyCollection<SaveOrderItemRequest> items,
        CancellationToken cancellationToken)
    {
        if (items.Count == 0)
            throw new BusinessException("An order needs at least one item.");

        var productIds = items.Select(item => item.ProductId).ToArray();
        if (productIds.Distinct().Count() != productIds.Length)
            throw new BusinessException("A product can appear only once in an order.");

        var products = await dbContext.Products
            .Where(product => productIds.Contains(product.Id))
            .ToDictionaryAsync(product => product.Id, cancellationToken);
        if (products.Count != productIds.Length)
            throw new BusinessException("One or more products do not exist.");

        foreach (var item in items)
        {
            var product = products[item.ProductId];
            if (!product.IsActive)
                throw new BusinessException($"Product {product.Name} is inactive.");
            if (item.Quantity > product.StockQuantity)
                throw new BusinessException(
                    $"Only {product.StockQuantity} units of {product.Name} are available.");
        }

        return products;
    }

    private async Task<TResult> ExecuteInTransactionAsync<TResult>(
        Func<Task<TResult>> action,
        CancellationToken cancellationToken)
    {
        var strategy = dbContext.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                var result = await action();
                await transaction.CommitAsync(cancellationToken);
                return result;
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        });
    }

    private static void ReplaceItems(
        Order order,
        IEnumerable<SaveOrderItemRequest> itemRequests,
        IReadOnlyDictionary<long, Product> products)
    {
        order.Items.Clear();
        foreach (var itemRequest in itemRequests)
        {
            var product = products[itemRequest.ProductId];
            order.Items.Add(new OrderItem
            {
                ProductId = product.Id,
                Product = product,
                Quantity = itemRequest.Quantity,
                UnitPrice = product.Price
            });
        }

        order.RecalculateTotal();
    }

    private static string CreateOrderNumber() =>
        $"ORD-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid():N}"[..26].ToUpperInvariant();

    private static OrderDetailsDto Map(Order order) =>
        new(
            order.Id,
            order.OrderNumber,
            order.CustomerId,
            order.Customer?.Name ?? string.Empty,
            order.OrderDateUtc,
            order.Status,
            order.TotalAmount,
            order.Items.Select(item => new OrderItemDto(
                item.Id,
                item.ProductId,
                item.Product?.Name ?? string.Empty,
                item.Product?.Sku ?? string.Empty,
                item.Quantity,
                item.UnitPrice,
                item.LineTotal)).ToArray());
}
