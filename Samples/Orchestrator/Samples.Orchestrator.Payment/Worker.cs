using System.Text.Json.Nodes;
using Samples.Orchestrator.Payment.Infrastructure.MessageBroker;
using Bogus;
using Confluent.Kafka;
using Samples.Orchestrator.Core.Domain.Events;
using Samples.Orchestrator.Core.Domain.Events.Payment;

namespace Samples.Orchestrator.Payment;

public class Worker(ILogger<Worker> logger, IServiceScopeFactory scopeFactory) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var scope = scopeFactory.CreateScope();
        var submittedProducer = scope.ServiceProvider.GetRequiredService<SubmittedProducer>();
        
        logger.LogInformation("Worker started at: {time}", DateTimeOffset.Now);
        
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var message = BuildMessage();
                await submittedProducer.PublishAsync(message, stoppingToken);
                
                logger.LogInformation("Published message at: {time}", DateTimeOffset.Now);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while publishing message ate: {time}", DateTimeOffset.Now);
            }
            
            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
        
        logger.LogInformation("Worker stopped at: {time}", DateTimeOffset.Now);
    }

    private static Submitted BuildMessage()
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

        var message = new Faker<Submitted>()
            .RuleFor(m => m.CorrelationId, f => f.Random.Guid())
            .RuleFor(m => m.CurrentState, "PaymentSubmitted")
            .RuleFor(m => m.Payload, payload)
            .Generate();
        
        return message;
    }
}