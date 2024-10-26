using MassTransit;

namespace Samples.Orchestrator.Api.Infrastructure.StateMachine;

public class OrderState : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public string? CurrentState { get; set; }
    public DateTime OrderDate { get; set; }
    public uint RowVersion { get; set; }
}