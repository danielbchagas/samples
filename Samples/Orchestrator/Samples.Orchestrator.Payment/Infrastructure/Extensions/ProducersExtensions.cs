using Samples.Orchestrator.Payment.Infrastructure.MessageBroker;

namespace Samples.Orchestrator.Payment.Infrastructure.Extensions;

public static class ProducersExtensions
{
    public static void AddProducers(this IServiceCollection services)
    {
        services.AddScoped<ISubmittedProducer, SubmittedProducer>();
        services.AddScoped<IAcceptedProducer, AcceptedProducer>();
        services.AddScoped<IRollbackProducer, RollbackProducer>();
        services.AddScoped<ICancelledProducer, CancelledProducer>();
        
        services.AddScoped<SubmittedProducer>();
        services.AddScoped<AcceptedProducer>();
        services.AddScoped<RollbackProducer>();
        services.AddScoped<CancelledProducer>();
    }
}