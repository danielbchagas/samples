namespace Samples.Orchestrator.BuildingBlocks.Events.Payment;

public record Cancelled : SagaEvent
{
    public required string Reason { get; set; }
}