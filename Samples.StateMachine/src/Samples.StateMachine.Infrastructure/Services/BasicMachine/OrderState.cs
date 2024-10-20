using MassTransit;

namespace Samples.StateMachine.Infrastructure.Services.BasicMachine;

public class OrderState : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public string? CurrentState { get; set; }
    public int RetryCount { get; set; }
    
    
    public Guid OrderId { get; set; }
    public string? OrderName { get; set; }
    public string? OrderDescription { get; set; }
    public decimal OrderAmount { get; set; }
}