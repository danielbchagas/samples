using MassTransit;
using Samples.Orchestrator.Core.Domain.Events;

namespace Samples.Orchestrator.Core.Infrastructure.StateMachine;

public class OrderState : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public string? CurrentState { get; set; }
    
    public int OrderId { get; set; }
    public string? Reason { get; set; }
    public string? Error { get; set; }
    public DateTime CreatedAt { get; set; }
    
    public void Initialize<T>(T message) where T : SagaEvent
    {
        CorrelationId = message.CorrelationId;
        CurrentState = message.CurrentState;
        OrderId = message.OrderId;
        Reason = message.Reason;
        Error = message.Error;
        CreatedAt = DateTime.UtcNow;
    }
}