using MassTransit;
using Samples.Orchestrator.Api;
using Samples.Orchestrator.Api.Domain.Events.Payment;
using Samples.Orchestrator.Api.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddMasstransit(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapPost("/payment/submit", async (IPublishEndpoint bus, Guid correlationId) =>
{
    await bus.Publish(new Submitted { CorrelationId = correlationId });
    return Results.Accepted($"Payment submission event sent with CorrelationId: {correlationId}");
})
.WithName("SubmitPayment")
.WithOpenApi();

app.MapPost("/payment/accept", async (IPublishEndpoint bus, Guid correlationId) =>
{
    await bus.Publish(new Accepted { CorrelationId = correlationId });
    return Results.Accepted($"Payment accepted event sent with CorrelationId: {correlationId}");
})
.WithName("AcceptPayment")
.WithOpenApi();

app.MapPost("/payment/cancel", async (IPublishEndpoint bus, Guid correlationId) =>
{
    await bus.Publish(new Cancelled { CorrelationId = correlationId });
    return Results.Accepted($"Payment cancelled event sent with CorrelationId: {correlationId}");
})
.WithName("CancelPayment")
.WithOpenApi();

app.MapPost("/payment/rollback", async (IPublishEndpoint bus, Guid correlationId) =>
{
    await bus.Publish(new Rollback { CorrelationId = correlationId });
    return Results.Accepted($"Payment rollback event sent with CorrelationId: {correlationId}");
})
.WithName("RollbackPayment")
.WithOpenApi();

app.Run();