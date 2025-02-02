namespace Samples.Orchestrator.BuildingBlocks.Events.Shipping;

public record Cancelled : SagaEvent
{
    public required string Reason { get; set; }
}