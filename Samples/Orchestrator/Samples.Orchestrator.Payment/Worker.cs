using System.Text.Json.Nodes;
using Samples.Orchestrator.Payment.Infrastructure.MessageBroker;
using Bogus;
using Samples.Orchestrator.Core.Domain.Events;
using Samples.Orchestrator.Core.Domain.Events.Payment;

namespace Samples.Orchestrator.Payment;

public class Worker(ILogger<Worker> logger, IServiceScopeFactory scopeFactory) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var scope = scopeFactory.CreateScope();
        var submittedProducer = scope.ServiceProvider.GetRequiredService<SubmittedProducer>();
        var cancelledProducer = scope.ServiceProvider.GetRequiredService<CancelledProducer>();
        
        while (!stoppingToken.IsCancellationRequested)
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            }

            try
            {
                var message = BuildMessage<Submitted>();
                await submittedProducer.PublishAsync(message, stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while publishing message");
                
                var message = BuildMessage<Cancelled>();
                await cancelledProducer.PublishAsync(message, stoppingToken);
            }
            
            await Task.Delay(1000, stoppingToken);
        }
    }

    private static T BuildMessage<T>() where T : SagaEvent
    {
        var payload = new Faker<JsonObject>()
            .CustomInstantiator(f => new JsonObject
            {
                ["OrderId"] = f.Random.Guid(),
                ["Amount"] = f.Random.Decimal(1, 1000),
                ["Currency"] = "BTC",
                ["WalletAddress"] = f.Finance.BitcoinAddress()
            })
            .Generate();

        var message = new Faker<T>()
            .RuleFor(m => m.CorrelationId, f => f.Random.Guid())
            .RuleFor(m => m.CurrentState, "PaymentSubmitted")
            .RuleFor(m => m.Payload, payload)
            .Generate();
        
        return message;
    }
}