using Samples.EventBus;
using Samples.EventBus.Application.Handler;
using Samples.EventBus.Infrastructure.Bus;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();

var eventBus = new InMemoryEventBus();
builder.Services.AddSingleton(eventBus);

builder.Services.AddSingleton<SubmittedHandler>();
var serviceProvider = builder.Services.BuildServiceProvider();

// Obter handler e assinar evento
var produtoCriadoHandler = serviceProvider.GetRequiredService<SubmittedHandler>();
eventBus.OnEventPublished += produtoCriadoHandler.OnSubmitted!;

var host = builder.Build();
host.Run();