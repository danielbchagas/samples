using MassTransit;

namespace Samples.Orchestrator.BuildingBlocks.Events;

public record SagaEvent : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    private string? CurrentState { get; set; }
    private DateTime? OrderDate { get; set; }

    public void Init(SagaEvent message)
    {
        CorrelationId = message.CorrelationId;
        CurrentState = message.CurrentState;
        OrderDate = message.OrderDate;
    }
}