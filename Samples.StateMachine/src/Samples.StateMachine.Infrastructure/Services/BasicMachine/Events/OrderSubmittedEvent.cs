namespace Samples.StateMachine.Infrastructure.Services.BasicMachine.Events;

public class OrderSubmittedEvent
{
    public Guid OrderId { get; set; }
    public string OrderName { get; set; }
    public decimal OrderAmount { get; set; }
    
    public OrderSubmittedEvent(Guid orderId, string orderName, decimal orderAmount)
    {
        OrderId = orderId;
        OrderName = orderName;
        OrderAmount = orderAmount;
    }
}