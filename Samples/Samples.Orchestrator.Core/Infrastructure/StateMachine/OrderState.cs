using MassTransit;
using Samples.Orchestrator.BuildingBlocks.Events;

namespace Samples.Orchestrator.Core.Infrastructure.StateMachine;

public class OrderState : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public string? CurrentState { get; set; }
    
    public int OrderId { get; set; }
    public int PaymentId { get; set; }
    public DateTime? OrderDate { get; set; }

    public void Init(SagaEvent message)
    {
        CorrelationId = message.CorrelationId;
        CurrentState = message.CurrentState;
        
        OrderId = message.OrderId;
        PaymentId = message.PaymentId;
        OrderDate = DateTime.UtcNow;
    }
}