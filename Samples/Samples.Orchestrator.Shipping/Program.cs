using MassTransit;
using Samples.Orchestrator.BuildingBlocks.Events.Payment;
using Submitted = Samples.Orchestrator.BuildingBlocks.Events.Shipping.Submitted;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapPost("/submitted", async (ITopicProducer<Submitted> producer, Submitted data) =>
    {
        await producer.Produce(data);
        return Results.Accepted();
    })
    .WithName("Submitted")
    .WithOpenApi();

app.MapPost("/accepted", async (ITopicProducer<Accepted> producer, Accepted data) =>
    {
        await producer.Produce(data);
        return Results.Accepted();
    })
    .WithName("Accepted")
    .WithOpenApi();

app.MapPost("/rollback", async (ITopicProducer<Rollback> producer, Rollback data) =>
    {
        await producer.Produce(data);
        return Results.Accepted();
    })
    .WithName("Rollback")
    .WithOpenApi();

app.MapPost("/cancelled", async (ITopicProducer<Cancelled> producer, Cancelled data) =>
    {
        await producer.Produce(data);
        return Results.Accepted();
    })
    .WithName("Cancelled")
    .WithOpenApi();

app.Run();
