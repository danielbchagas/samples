using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Samples.Orchestrator.Payment.Domain.Order;
using Samples.Orchestrator.Payment.Infrastructure.Database;

namespace Samples.Orchestrator.Payment.Infrastructure.Repositories;

public class OrderRepository(PaymentDbContext dbContext) : IOrderRepository
{
    public async Task<IEnumerable<Order>> GetAsync(Expression<Func<Order, bool>> predicate, CancellationToken cancellationToken)
    {
        return await dbContext.Orders
            .Where(predicate)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Order order, CancellationToken cancellationToken)
    {
        dbContext.Orders.Add(order);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Order order, CancellationToken cancellationToken)
    {
        dbContext.Orders.Update(order);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Order order, CancellationToken cancellationToken)
    {
        dbContext.Orders.Remove(order);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}