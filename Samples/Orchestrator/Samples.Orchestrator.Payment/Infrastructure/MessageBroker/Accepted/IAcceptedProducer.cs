using System.Text.Json.Nodes;
using Samples.Orchestrator.Core.Domain.Events.Payment;

namespace Samples.Orchestrator.Payment.Infrastructure.MessageBroker;

public interface IAcceptedProducer
{
    Task PublishAsync(Accepted cancelledEvent, JsonObject payload);
}