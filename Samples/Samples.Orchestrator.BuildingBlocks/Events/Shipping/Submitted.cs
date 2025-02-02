namespace Samples.Orchestrator.BuildingBlocks.Events.Shipping;

public record Submitted : SagaEvent
{
    public int OrderId { get; set; }
    public int PaymentId { get; set; }
}