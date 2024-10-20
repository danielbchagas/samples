namespace Samples.StateMachine.Infrastructure.Services.BasicMachine.Events;

public class OrderRejectedEvent
{
    public Guid OrderId { get; set; }
}