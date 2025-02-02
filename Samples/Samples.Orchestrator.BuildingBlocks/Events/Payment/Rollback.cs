namespace Samples.Orchestrator.BuildingBlocks.Events.Payment;

public record Rollback : SagaEvent
{
    public required Exception Exception { get; set; }
}