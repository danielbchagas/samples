namespace Samples.Orchestrator.BuildingBlocks.Events.Shipping;

public record Rollback : SagaEvent
{
    public required Exception Exception { get; set; }
}