using MassTransit;

namespace Samples.Orchestrator.Core.Domain.Events;

public record SagaEvent : ISaga
{
    public Guid CorrelationId { get; set; }
    public string? CurrentState { get; set; }
    
    public int OrderId { get; set; }
    public string? Reason { get; set; }
    public string? Error { get; set; }
    public DateTime CreatedAt { get; set; }
}