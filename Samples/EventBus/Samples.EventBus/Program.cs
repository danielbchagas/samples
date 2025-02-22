using Samples.EventBus;
using Samples.EventBus.Application.Handler;
using Samples.EventBus.Infrastructure.Bus;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();

var eventBus = new InMemoryEventBus();
builder.Services.AddSingleton(eventBus);

builder.Services.AddSingleton<SubmittedHandler>();
var serviceProvider = builder.Services.BuildServiceProvider();

// Register the handler and subscribe to the event (eventBus)
var submittedHandler = serviceProvider.GetRequiredService<SubmittedHandler>();
eventBus.OnEventPublished += submittedHandler.OnSubmitted!;

var host = builder.Build();
host.Run();