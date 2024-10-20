using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Samples.StateMachine.Infrastructure.Extensions;
using Samples.StateMachine.Infrastructure.Services.BasicMachine.Events;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddMasstransit();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Endpoint para submeter um pedido
app.MapPost("/submit", async ([FromServices] IBus bus, [FromBody] OrderSubmittedEvent request) =>
    {
        if (string.IsNullOrWhiteSpace(request.OrderName) || request.OrderAmount <= 0)
            return Results.BadRequest("OrderName e OrderAmount são obrigatórios.");

        var orderId = Guid.NewGuid();

        // Publica o evento OrderSubmittedEvent
        await bus.Publish(request);

        return Results.Accepted($"/orders/{orderId}", new { OrderId = orderId });
    })
    .WithName("SubmitOrder")
    .WithOpenApi();

// Endpoint para aceitar um pedido
app.MapPut("/accept/{orderId:guid}", async ([FromServices] IBus bus, Guid orderId) =>
    {
        await bus.Publish(new OrderAcceptedEvent { OrderId = orderId });
        return Results.Ok($"Pedido {orderId} aceito.");
    })
    .WithName("AcceptOrder")
    .WithOpenApi();

// Endpoint para rejeitar um pedido
app.MapPut("/reject/{orderId:guid}", async ([FromServices] IBus bus, Guid orderId) =>
    {
        await bus.Publish(new OrderRejectedEvent { OrderId = orderId });
        return Results.Ok($"Pedido {orderId} rejeitado.");
    })
    .WithName("RejectOrder")
    .WithOpenApi();

app.Run();