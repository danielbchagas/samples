using System.Text.Json.Nodes;
using Samples.Orchestrator.Core.Domain.Events.Payment;

namespace Samples.Orchestrator.Payment.Infrastructure.MessageBroker;

public interface IRollbackProducer
{
    Task PublishAsync(Rollback rollbackEvent, CancellationToken cancellationToken = default);
}