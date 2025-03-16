using System.Text.Json.Nodes;
using MassTransit;
using Samples.Orchestrator.Core.Domain.Events.Payment;

namespace Samples.Orchestrator.Payment.Infrastructure.MessageBroker;

public class RollbackProducer(ILogger<RollbackProducer> logger, ISendEndpointProvider sendEndpointProvider) : IRollbackProducer
{
    public async Task PublishAsync(Rollback rollbackEvent, JsonObject payload)
    {
        rollbackEvent.Payload = payload;
        
        var endpoint = await sendEndpointProvider.GetSendEndpoint(new Uri("queue:saga.payment.rollback"));
        await endpoint.Send(rollbackEvent);
        
        logger.LogInformation("Rollback event published");
    }
}