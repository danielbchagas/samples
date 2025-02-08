using MassTransit;
using Samples.Orchestrator.BuildingBlocks.Events;

namespace Samples.Orchestrator.Core.Infrastructure.StateMachine;

public class OrderState : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public string? CurrentState { get; set; }
    
    public int OrderId { get; set; }
    public string? Reason { get; set; }
    public Exception? Exception { get; set; }
    public DateTime? CreatedAt { get; set; }

    public void Init(SagaEvent message)
    {
        CorrelationId = message.CorrelationId;
        CurrentState = message.CurrentState;
        
        OrderId = message.OrderId;
        CreatedAt = DateTime.UtcNow;
    }
}