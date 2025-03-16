using System.Text.Json.Nodes;
using MassTransit;
using Samples.Orchestrator.Core.Domain.Events.Payment;

namespace Samples.Orchestrator.Payment.Infrastructure.MessageBroker;

public class SubmittedProducer(ILogger<SubmittedProducer> logger, ISendEndpointProvider sendEndpointProvider) : ISubmittedProducer
{
    public async Task PublishAsync(Submitted submittedEvent, CancellationToken cancellationToken)
    {
        var endpoint = await sendEndpointProvider.GetSendEndpoint(new Uri("queue:saga.payment.submitted"));
        await endpoint.Send(submittedEvent, cancellationToken);
        
        logger.LogInformation("Submitted event published");
    }
}