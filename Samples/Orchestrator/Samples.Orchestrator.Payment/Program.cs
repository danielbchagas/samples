using Samples.Orchestrator.Payment;
using Samples.Orchestrator.Payment.Infrastructure.Extensions;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();

var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables()
    .Build();

builder.Services.AddEntityFramework(configuration);
builder.Services.AddMasstransit(configuration);
builder.Services.AddProducers();

var host = builder.Build();
host.Run();