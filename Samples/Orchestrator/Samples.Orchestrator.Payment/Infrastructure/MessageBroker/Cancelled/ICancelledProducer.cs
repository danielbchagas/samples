using System.Text.Json.Nodes;
using Samples.Orchestrator.Core.Domain.Events.Payment;

namespace Samples.Orchestrator.Payment.Infrastructure.MessageBroker;

public interface ICancelledProducer
{
    Task PublishAsync(Cancelled cancelledEvent, CancellationToken cancellationToken = default);
}