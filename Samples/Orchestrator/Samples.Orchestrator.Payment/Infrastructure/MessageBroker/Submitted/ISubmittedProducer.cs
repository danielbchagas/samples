using System.Text.Json.Nodes;
using Samples.Orchestrator.Core.Domain.Events.Payment;

namespace Samples.Orchestrator.Payment.Infrastructure.MessageBroker;

public interface ISubmittedProducer
{
    Task PublishAsync(Submitted submittedEvent, CancellationToken cancellationToken = default);
}