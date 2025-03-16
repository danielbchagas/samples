namespace Samples.Orchestrator.Core.Domain.Settings;

internal record BrokerSettings
{
    public required string Host { get; set; }
    public required string Port { get; set; }
    public required string Username { get; set; }
    public required string Password { get; set; }
    public required Endpoints Endpoints { get; set; }
}

internal record Endpoints
{
    public required string PaymentSubmitted { get; set; }
    public required string PaymentAccepted { get; set; }
    public required string PaymentCancelled { get; set; }
    public required string PaymentRollback { get; set; }
    public required string PaymentProcessing { get; set; }
    public required string ShippingSubmitted { get; set; }
    public required string ShippingAccepted { get; set; }
    public required string ShippingCancelled { get; set; }
    public required string ShippingRollback { get; set; }
    public required string ShippingProcessing { get; set; }
}