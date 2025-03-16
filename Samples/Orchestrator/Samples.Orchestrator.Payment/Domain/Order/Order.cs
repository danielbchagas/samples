namespace Samples.Orchestrator.Payment.Domain.Order;

public class Order : IOrder
{
    public Order()
    {
        
    }
    
    public Order(int id, decimal total, DateTime createdAt)
    {
        Id = id;
        Total = total;
        CreatedAt = createdAt;
    }
    
    public int Id { get; set; }
    public decimal Total { get; set; }
    public DateTime CreatedAt { get; set; }
}