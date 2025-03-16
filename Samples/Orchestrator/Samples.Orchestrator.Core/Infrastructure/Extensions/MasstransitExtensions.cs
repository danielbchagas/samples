using MassTransit;
using MassTransit.EntityFrameworkCoreIntegration;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using Samples.Orchestrator.Core.Domain.Settings;
using Samples.Orchestrator.Core.Infrastructure.Database;
using Samples.Orchestrator.Core.Infrastructure.StateMachine;
using Payment = Samples.Orchestrator.Core.Domain.Events.Payment;
using Shipping = Samples.Orchestrator.Core.Domain.Events.Shipping;

namespace Samples.Orchestrator.Core.Infrastructure.Extensions;

/*
 * https://masstransit.io/
 * https://masstransit.io/documentation/configuration/transports/rabbitmq
 * https://masstransit.io/documentation/transports/rabbitmq
 */

public static class MasstransitExtensions
{
    public static IServiceCollection AddMasstransit(this IServiceCollection services, IConfiguration configuration)
    {
        var settings = BuildConfig(configuration);
        
        services.AddDbContext<DbContext, OrderStateDbContext>(opt =>
        {
            opt.UseNpgsql(configuration.GetConnectionString("DefaultConnection"), options => options.EnableRetryOnFailure());
        });
        
        services.AddMassTransit(cfg =>
        {
            cfg.AddSagaStateMachine<OrderStateMachine, OrderState>()
                .EntityFrameworkRepository(r =>
                {
                    r.ConcurrencyMode = ConcurrencyMode.Pessimistic;
            
                    r.AddDbContext<SagaDbContext, OrderStateDbContext>((provider,builder) =>
                    {
                        builder.UseNpgsql(configuration.GetConnectionString("DefaultConnection"), options =>
                        {
                            // options.MigrationsAssembly(Assembly.GetExecutingAssembly().GetName().Name);
                            // options.MigrationsHistoryTable($"__{nameof(OrderStateDbContext)}");
                            options.EnableRetryOnFailure();
                        });
                    });
                    
                    r.UsePostgres();
                });
            
            cfg.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(settings.Host, "/", h =>
                {
                    h.Username(settings.Username);
                    h.Password(settings.Password);
                });
                
                cfg.ConfigureEndpoints(context);
                cfg.UseRawJsonDeserializer(isDefault: true);
                
                cfg.ConfigureJsonSerializerOptions(opts =>
                {
                    opts.PropertyNameCaseInsensitive = true;
                    return opts;
                });
                
                #region Payment
                cfg.ReceiveEndpoint(settings.Endpoints.PaymentSubmitted, e =>
                {
                    e.ExchangeType = ExchangeType.Fanout;
                    e.Bind<Payment.Submitted>();
                    e.UseMessageRetry(retryConfig => retryConfig.Interval(3, TimeSpan.FromSeconds(5)));
                    e.ConfigureSaga<OrderState>(context);
                });

                cfg.ReceiveEndpoint(settings.Endpoints.PaymentAccepted, e =>
                {
                    e.ExchangeType = ExchangeType.Fanout;
                    e.Bind<Payment.Accepted>();
                    e.UseMessageRetry(retryConfig => retryConfig.Interval(3, TimeSpan.FromSeconds(5)));
                    e.ConfigureSaga<OrderState>(context);
                });

                cfg.ReceiveEndpoint(settings.Endpoints.PaymentRollback, e =>
                {
                    e.ExchangeType = ExchangeType.Fanout;
                    e.Bind<Payment.Rollback>();
                    e.UseMessageRetry(retryConfig => retryConfig.Interval(3, TimeSpan.FromSeconds(5)));
                    e.ConfigureSaga<OrderState>(context);
                });
                
                cfg.ReceiveEndpoint(settings.Endpoints.PaymentCancelled, e =>
                {
                    e.ExchangeType = ExchangeType.Fanout;
                    e.Bind<Payment.Cancelled>();
                    e.UseMessageRetry(retryConfig => retryConfig.Interval(3, TimeSpan.FromSeconds(5)));
                    e.ConfigureSaga<OrderState>(context);
                });
                #endregion

                #region Shipping
                cfg.ReceiveEndpoint(settings.Endpoints.ShippingSubmitted, e =>
                {
                    e.ExchangeType = ExchangeType.Fanout;
                    e.Bind<Shipping.Submitted>();
                    e.UseMessageRetry(retryConfig => retryConfig.Interval(3, TimeSpan.FromSeconds(5)));
                    e.ConfigureSaga<OrderState>(context);
                });

                cfg.ReceiveEndpoint(settings.Endpoints.ShippingAccepted, e =>
                {
                    e.ExchangeType = ExchangeType.Fanout;
                    e.Bind<Shipping.Accepted>();
                    e.UseMessageRetry(retryConfig => retryConfig.Interval(3, TimeSpan.FromSeconds(5)));
                    e.ConfigureSaga<OrderState>(context);
                });

                cfg.ReceiveEndpoint(settings.Endpoints.ShippingRollback, e =>
                {
                    e.ExchangeType = ExchangeType.Fanout;
                    e.Bind<Shipping.Rollback>();
                    e.UseMessageRetry(retryConfig => retryConfig.Interval(3, TimeSpan.FromSeconds(5)));
                    e.ConfigureSaga<OrderState>(context);
                });
                
                cfg.ReceiveEndpoint(settings.Endpoints.ShippingCancelled, e =>
                {
                    e.ExchangeType = ExchangeType.Fanout;
                    e.Bind<Shipping.Cancelled>();
                    e.UseMessageRetry(retryConfig => retryConfig.Interval(3, TimeSpan.FromSeconds(5)));
                    e.ConfigureSaga<OrderState>(context);
                });
                #endregion
            });
        });
            
        return services;
    }
    
    private static BrokerSettings BuildConfig(IConfiguration configuration)
    {
        var settings = configuration.GetSection("Broker").Get<BrokerSettings>();
        
        ArgumentNullException.ThrowIfNull(settings);
        
        ArgumentException.ThrowIfNullOrEmpty(settings.Host);
        ArgumentException.ThrowIfNullOrEmpty(settings.Port);
        ArgumentException.ThrowIfNullOrEmpty(settings.Username);
        ArgumentException.ThrowIfNullOrEmpty(settings.Password);
        
        ArgumentNullException.ThrowIfNull(settings.Endpoints);
        
        ArgumentException.ThrowIfNullOrEmpty(settings.Endpoints.PaymentSubmitted);
        ArgumentException.ThrowIfNullOrEmpty(settings.Endpoints.PaymentAccepted);
        ArgumentException.ThrowIfNullOrEmpty(settings.Endpoints.PaymentCancelled);
        ArgumentException.ThrowIfNullOrEmpty(settings.Endpoints.PaymentRollback);
        ArgumentException.ThrowIfNullOrEmpty(settings.Endpoints.PaymentProcessing);
        
        ArgumentException.ThrowIfNullOrEmpty(settings.Endpoints.ShippingSubmitted);
        ArgumentException.ThrowIfNullOrEmpty(settings.Endpoints.ShippingAccepted);
        ArgumentException.ThrowIfNullOrEmpty(settings.Endpoints.ShippingCancelled);
        ArgumentException.ThrowIfNullOrEmpty(settings.Endpoints.ShippingRollback);
        ArgumentException.ThrowIfNullOrEmpty(settings.Endpoints.ShippingProcessing);
        
        return settings;
    }
}
