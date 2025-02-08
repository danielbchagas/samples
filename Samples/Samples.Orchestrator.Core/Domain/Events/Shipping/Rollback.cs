namespace Samples.Orchestrator.Core.Domain.Events.Shipping;

public record Rollback : SagaEvent
{
    public required Exception Exception { get; set; }
}