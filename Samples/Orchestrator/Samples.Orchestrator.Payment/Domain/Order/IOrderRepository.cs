using System.Linq.Expressions;

namespace Samples.Orchestrator.Payment.Domain.Order;

public interface IOrderRepository
{
    Task<IEnumerable<Order>> GetAsync(Expression<Func<Order, bool>> predicate, CancellationToken cancellationToken = default);
    Task AddAsync(Order order, CancellationToken cancellationToken = default);
    Task UpdateAsync(Order order, CancellationToken cancellationToken = default);
    Task DeleteAsync(Order order, CancellationToken cancellationToken = default);
}