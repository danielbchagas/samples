using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using Samples.Orchestrator.Core.Infrastructure.Database;
using Samples.Orchestrator.Core.Infrastructure.Extensions;
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

app.MapPost("/payment-submitted", async ([FromServices] IPublishEndpoint producer, Payment.Submitted data) =>
    {
        await producer.Publish<Payment.Submitted>(data);
        return Results.Accepted();
    })
    .WithName("Payment Submitted")
    .WithOpenApi();

app.MapPost("/payment-cancelled", async ([FromServices] IPublishEndpoint producer, Payment.Cancelled data) =>
    {
        await producer.Publish<Payment.Cancelled>(data);
        return Results.Accepted();
    })
    .WithName("Payment Cancelled")
    .WithOpenApi();

// app.MapPost("/shipping-submitted", async ([FromServices] ITopicProducer<Shipping.Submitted> producer, Shipping.Submitted data) =>
//     {
//         await producer.Produce(data);
//         return Results.Accepted();
//     })
//     .WithName("Shipping Submitted")
//     .WithOpenApi();

// app.MapPost("/shipping-cancelled", async ([FromServices] ITopicProducer<Shipping.Cancelled> producer, Shipping.Cancelled data) =>
//     {
//         await producer.Produce(data);
//         return Results.Accepted();
//     })
//     .WithName("Shipping Cancelled")
//     .WithOpenApi();

app.Run();