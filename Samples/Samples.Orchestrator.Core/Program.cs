using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Samples.Orchestrator.Core.Infrastructure.Database;
using Samples.Orchestrator.Core.Infrastructure.Extensions;
using Samples.Orchestrator.Core.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddMasstransit(builder.Configuration);
builder.Services.AddHostedService<PaymentWorker>();
builder.Services.AddHostedService<ShippingWorker>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/order-states/{id}", ([FromServices] OrderStateDbContext context, [FromQuery] Guid id) =>
    {
        var result = context.OrderStates.FindAsync(id);
        return Results.Ok(result);
    })
    .WithName("Get Order")
    .WithOpenApi();

app.MapGet("/order-states/{page}/{pageSize}", ([FromServices] OrderStateDbContext context, [FromQuery] int page, int pageSize) =>
    {
        var result = context.OrderStates.AsNoTracking().Skip((page - 1) * pageSize).Take(pageSize);
        return Results.Ok(result);
    })
    .WithName("Get Orders")
    .WithOpenApi();

app.Run();