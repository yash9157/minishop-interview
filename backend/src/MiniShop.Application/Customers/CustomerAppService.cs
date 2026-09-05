using Microsoft.EntityFrameworkCore;
using MiniShop.Application.Contracts;
using MiniShop.Domain;
using MiniShop.EntityFrameworkCore;

namespace MiniShop.Application;

public sealed class CustomerAppService(MiniShopDbContext dbContext) : ICustomerAppService
{
    public async Task<PagedResult<CustomerDto>> GetListAsync(
        PagedRequest request,
        CancellationToken cancellationToken)
    {
        var query = dbContext.Customers.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim();
            query = query.Where(customer =>
                customer.Name.Contains(search) || customer.Email.Contains(search));
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var customers = await query
            .OrderBy(customer => customer.Name)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(customer => new CustomerDto(
                    customer.Id,
                    customer.Name,
                    customer.Email,
                    customer.Phone))
            .ToListAsync(cancellationToken);

        return new PagedResult<CustomerDto>(
            customers,
            totalCount,
            request.Page,
            request.PageSize);
    }

    public async Task<CustomerDto> GetAsync(long id, CancellationToken cancellationToken) =>
        Map(await FindAsync(id, cancellationToken));

    public async Task<CustomerDto> CreateAsync(
        SaveCustomerRequest request,
        CancellationToken cancellationToken)
    {
        var email = NormalizeEmail(request.Email);
        await EnsureEmailIsUniqueAsync(email, null, cancellationToken);

        var customer = new Customer
        {
            Name = request.Name.Trim(),
            Email = email,
            Phone = request.Phone?.Trim()
        };
        dbContext.Add(customer);
        await dbContext.SaveChangesAsync(cancellationToken);
        return Map(customer);
    }

    public async Task<CustomerDto> UpdateAsync(
        long id,
        SaveCustomerRequest request,
        CancellationToken cancellationToken)
    {
        var customer = await FindAsync(id, cancellationToken);
        var email = NormalizeEmail(request.Email);
        await EnsureEmailIsUniqueAsync(email, id, cancellationToken);

        customer.Name = request.Name.Trim();
        customer.Email = email;
        customer.Phone = request.Phone?.Trim();
        await dbContext.SaveChangesAsync(cancellationToken);
        return Map(customer);
    }

    public async Task DeleteAsync(long id, CancellationToken cancellationToken)
    {
        var customer = await FindAsync(id, cancellationToken);
        if (await dbContext.Orders.AnyAsync(
                order => order.CustomerId == id,
                cancellationToken))
            throw new ConflictException("Customer has orders.");

        dbContext.Remove(customer);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task<Customer> FindAsync(long id, CancellationToken cancellationToken) =>
        await dbContext.Customers.FirstOrDefaultAsync(
            customer => customer.Id == id,
            cancellationToken)
        ?? throw new NotFoundException("Customer was not found.");

    private async Task EnsureEmailIsUniqueAsync(
        string email,
        long? excludingId,
        CancellationToken cancellationToken)
    {
        var exists = await dbContext.Customers.AnyAsync(
            customer => customer.Email == email && (!excludingId.HasValue || customer.Id != excludingId),
            cancellationToken);
        if (exists)
            throw new ConflictException("Customer email already exists.");
    }

    private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();

    private static CustomerDto Map(Customer customer) =>
        new(customer.Id, customer.Name, customer.Email, customer.Phone);
}
