namespace Samples.Orchestrator.Core.Domain.States;

/// <summary>
/// Representa os possíveis estados do fluxo de orquestração.
/// </summary>
public enum OrderStatus
{
    PaymentSubmitted,
    PaymentAccepted,
    PaymentCancelled,
    PaymentRollback,
    ShippingSubmitted,
    ShippingAccepted
}
