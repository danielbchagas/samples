namespace Samples.StateMachine.Domain.Orders;

public class Order
{
    public Guid OrderId { get; set; }
    public string? OrderName { get; set; }
    public string? OrderDescription { get; set; }
    public decimal OrderAmount { get; set; }
}