using PaymentWorker = Samples.Orchestrator.Core.Services.Payment;

namespace Samples.Orchestrator.Core.Services.Extensions;

public static class WorkerExtensions
{
    public static IServiceCollection AddWorker(this IServiceCollection services)
    {
        services.AddHostedService<PaymentWorker.PaymentWorker>();

        services.AddScoped<PaymentWorker.AcceptedWorker>();
        services.AddScoped<PaymentWorker.CancelledWorker>();
        
        return services;
    }
}