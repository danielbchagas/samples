using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Samples.Orchestrator.Core.Infrastructure.Extensions;
using Samples.Orchestrator.Core.Transport.Payment;
using Samples.Orchestrator.Core.Transport.Shipping;
using Payment = Samples.Orchestrator.Core.Domain.Events.Payment;
using Shipping = Samples.Orchestrator.Core.Domain.Events.Shipping;

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

// app.MapGet("/order-states/{id}", ([FromServices] OrderStateDbContext context, [FromQuery] int id) =>
//     {
//         var result = context.OrderStates.FindAsync(id);
//         return Results.Ok(result);
//     })
//     .WithName("find-order-states-by-id")
//     .WithOpenApi();
//
// app.MapGet("/order-states/{state}", ([FromServices] OrderStateDbContext context, [FromQuery] string state) =>
//     {
//         var result = context.OrderStates.Where(o => o.CurrentState == state);
//         return Results.Ok(result);
//     })
//     .WithName("find-order-states-by-state")
//     .WithOpenApi();

app.MapPost("/payment-submitted", async ([FromServices] IPublishEndpoint producer, CreatePayment data) =>
    {
        var payment = data.MapToSubmitted();
        
        await producer.Publish<Payment.Submitted>(payment);
        return Results.Accepted();
    })
    .WithName("Payment Submitted")
    .WithOpenApi();

app.MapPost("/payment-cancelled", async ([FromServices] IPublishEndpoint producer, CancellPayment data) =>
    {
        var cancelled = data.MapToCancelled();
        
        await producer.Publish<Payment.Cancelled>(cancelled);
        return Results.Accepted();
    })
    .WithName("Payment Cancelled")
    .WithOpenApi();

app.MapPost("/shipping-submitted", async ([FromServices] IPublishEndpoint producer, CreateShipping data) =>
    {
        var payment = data.MapToSubmitted();
        
        await producer.Publish<Shipping.Submitted>(payment);
        return Results.Accepted();
    })
    .WithName("Shipping Submitted")
    .WithOpenApi();

app.MapPost("/shipping-cancelled", async ([FromServices] IPublishEndpoint producer, CancellShipping data) =>
    {
        var cancelled = data.MapToCancelled();
        
        await producer.Publish<Shipping.Cancelled>(cancelled);
        return Results.Accepted();
    })
    .WithName("Shipping Cancelled")
    .WithOpenApi();

app.Run();