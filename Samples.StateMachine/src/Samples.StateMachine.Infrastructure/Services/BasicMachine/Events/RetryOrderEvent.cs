namespace Samples.StateMachine.Infrastructure.Services.BasicMachine.Events;

public class RetryOrderEvent
{
    public Guid OrderId { get; set; }
}