using Bogus;
using Samples.EventBus.Domain.Events;
using Samples.EventBus.Infrastructure.Bus;

namespace Samples.EventBus;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly InMemoryEventBus _eventBus;

    public Worker(ILogger<Worker> logger, InMemoryEventBus inMemoryEventBus)
    {
        _logger = logger;
        _eventBus = inMemoryEventBus;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            if (_logger.IsEnabled(LogLevel.Information))
            {
                var faker = new Faker<Submitted>("pt_BR")
                    .RuleFor(p => p.Id, f => f.Random.Guid())
                    .RuleFor(p => p.Model, f => f.Vehicle.Model())
                    .RuleFor(p => p.Manufacturer, f => f.Vehicle.Manufacturer())
                    .RuleFor(p => p.Price, f => f.Random.Int(18, 80))
                    .RuleFor(p => p.CreatedAt, f => f.Date.Recent());
                
                var submitted = faker.Generate();
                _eventBus.Publish(submitted);
                
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            }

            await Task.Delay(1000, stoppingToken);
        }
    }
}