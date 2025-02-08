namespace Samples.Orchestrator.Core.Domain.Events.Payment;

public record Rollback : SagaEvent
{
    public required Exception Exception { get; set; }
}