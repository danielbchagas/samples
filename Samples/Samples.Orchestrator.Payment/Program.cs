using MassTransit;
using Samples.Orchestrator.BuildingBlocks.Events.Payment;

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

app.MapPost("/cancelled", async (ITopicProducer<Cancelled> producer, Cancelled data) =>
    {
        await producer.Produce(data);
        return Results.Accepted();
    })
    .WithName("Cancelled")
    .WithOpenApi();

app.Run();
