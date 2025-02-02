using Samples.Orchestrator.Core.Infrastructure.Database;
using Samples.Orchestrator.Core.Infrastructure.Extensions;

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

app.MapGet("/order-states/{id}", (OrderStateDbContext context, int id) =>
    {
        var result = context.OrderStates.FindAsync(id);
        return Results.Ok(result);
    })
    .WithName("find-order-states-by-id")
    .WithOpenApi();

app.MapGet("/order-states/{state}", (OrderStateDbContext context, string state) =>
    {
        var result = context.OrderStates.Where(o => o.CurrentState == state);
        return Results.Ok(result);
    })
    .WithName("find-order-states-by-state")
    .WithOpenApi();

app.Run();