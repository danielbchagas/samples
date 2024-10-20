namespace Samples.StateMachine.Infrastructure.Services.BasicMachine.Events;

public class OrderAcceptedEvent
{
    public Guid OrderId { get; set; }
}