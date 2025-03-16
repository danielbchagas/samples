namespace Samples.Orchestrator.Payment.Domain.Order;

public interface IOrder
{
    int Id { get; }
    decimal Total { get; }
    DateTime CreatedAt { get; }
}