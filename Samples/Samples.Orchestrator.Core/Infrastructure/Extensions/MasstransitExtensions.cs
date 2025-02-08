#define RABBITMQ

using System.Reflection;
using MassTransit;
using MassTransit.EntityFrameworkCoreIntegration;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using Payment = Samples.Orchestrator.Core.Domain.Events.Payment;
using Shipping = Samples.Orchestrator.Core.Domain.Events.Shipping;
using Samples.Orchestrator.Core.Domain.Settings;
using Samples.Orchestrator.Core.Infrastructure.Database;
using Samples.Orchestrator.Core.Infrastructure.StateMachine;

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
            opt.UseNpgsql(configuration.GetConnectionString("DefaultConnection"));
        });
        
        services.AddMassTransit(cfg =>
        {
            cfg.AddSagaStateMachine<OrderStateMachine, OrderState>()
                .EntityFrameworkRepository(r =>
                {
                    r.ConcurrencyMode = ConcurrencyMode.Pessimistic;
            
                    r.AddDbContext<SagaDbContext, OrderStateDbContext>((provider,builder) =>
                    {
                        builder.UseNpgsql(configuration.GetConnectionString("DefaultConnection"), m =>
                        {
                            m.MigrationsAssembly(Assembly.GetExecutingAssembly().GetName().Name);
                            m.MigrationsHistoryTable($"__{nameof(OrderStateDbContext)}");
                        });
                    });
                    
                    r.UsePostgres();
                });
            
            #if RABBITMQ
            cfg.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(settings.Host, "/", h =>
                {
                    h.Username(settings.Username);
                    h.Password(settings.Password);
                });
                
                #region Payment
                cfg.Publish<Payment.Submitted>(p =>
                {
                    p.BindQueue("orchestrator", settings.Endpoints.PaymentSubmitted, config =>
                    {
                        config.RoutingKey = "payment";
                        config.ExchangeType = ExchangeType.Direct;
                    });
                });
                
                cfg.Publish<Payment.Cancelled>(p =>
                {
                    p.BindQueue("orchestrator", settings.Endpoints.PaymentCancelled, config =>
                    {
                        config.RoutingKey = "payment";
                        config.ExchangeType = ExchangeType.Direct;
                    });
                });
                
                cfg.ReceiveEndpoint(settings.Endpoints.PaymentAccepted, e =>
                {
                    e.ExchangeType = ExchangeType.Direct;
                    e.Bind<Payment.Accepted>();
                    e.UseMessageRetry(retryConfig => retryConfig.Interval(3, TimeSpan.FromSeconds(5)));
                    e.ConfigureSaga<OrderState>(context);
                });

                cfg.ReceiveEndpoint(settings.Endpoints.PaymentRollback, e =>
                {
                    e.ExchangeType = ExchangeType.Direct;
                    e.Bind<Payment.Rollback>();
                    e.UseMessageRetry(retryConfig => retryConfig.Interval(3, TimeSpan.FromSeconds(5)));
                    e.ConfigureSaga<OrderState>(context);
                });
                #endregion

                #region Shipping
                cfg.Publish<Shipping.Submitted>(p =>
                {
                    p.BindQueue("orchestrator", settings.Endpoints.ShippingSubmitted, config =>
                    {
                        config.RoutingKey = "shipping";
                        config.ExchangeType = ExchangeType.Direct;
                    });
                });
                
                cfg.Publish<Shipping.Cancelled>(p =>
                {
                    p.BindQueue("orchestrator", settings.Endpoints.ShippingCancelled, config =>
                    {
                        config.RoutingKey = "shipping";
                        config.ExchangeType = ExchangeType.Direct;
                    });
                });
                
                cfg.ReceiveEndpoint(settings.Endpoints.ShippingAccepted, e =>
                {
                    e.ExchangeType = ExchangeType.Direct;
                    e.Bind<Shipping.Accepted>();
                    e.UseMessageRetry(retryConfig => retryConfig.Interval(3, TimeSpan.FromSeconds(5)));
                    e.ConfigureSaga<OrderState>(context);
                });

                cfg.ReceiveEndpoint(settings.Endpoints.ShippingRollback, e =>
                {
                    e.ExchangeType = ExchangeType.Direct;
                    e.Bind<Shipping.Rollback>();
                    e.UseMessageRetry(retryConfig => retryConfig.Interval(3, TimeSpan.FromSeconds(5)));
                    e.ConfigureSaga<OrderState>(context);
                });
                #endregion
            });
            #endif
            
            #if KAFKA
            cfg.AddRider(rider =>
            {
                rider.AddSagaStateMachine<OrderStateMachine, OrderState>();
                
                #region Payment
            
                rider.AddProducer<BuildingBlocks.Events.Payment.Submitted>("saga.pagamento.iniciar");
                rider.AddProducer<BuildingBlocks.Events.Payment.Rollback>("saga.pagamento.rollback");
            
                #endregion
            
                #region Shipping
            
                rider.AddProducer<BuildingBlocks.Events.Shipping.Submitted>("saga.envio.iniciar");
                rider.AddProducer<BuildingBlocks.Events.Shipping.Rollback>("saga.envio.rollback");
            
                #endregion
                
                rider.UsingKafka((context, k) =>
                {
                    k.Host("127.0.0.1:9092");
                    
                    #region Payment
                    
                    k.TopicEndpoint<BuildingBlocks.Events.Payment.Accepted>("saga.pagamento.confirmado", "saga-pagamento-group", e =>
                    {
                        e.UseMessageRetry(retryConfig => retryConfig.Interval(3, TimeSpan.FromSeconds(5)));
                        e.ConfigureSaga<OrderState>(context);
                    });
                    
                    k.TopicEndpoint<BuildingBlocks.Events.Payment.Cancelled>("saga.pagamento.cancelado", "saga-pagamento-group", e =>
                    {
                        e.UseMessageRetry(retryConfig => retryConfig.Interval(3, TimeSpan.FromSeconds(5)));
                        e.ConfigureSaga<OrderState>(context);
                    });
                    #endregion
                
                    #region Shipping
                    k.TopicEndpoint<BuildingBlocks.Events.Shipping.Accepted>("saga.envio.confirmado", "saga-envio-group", e =>
                    {
                        e.UseMessageRetry(retryConfig => retryConfig.Interval(3, TimeSpan.FromSeconds(5)));
                        e.ConfigureSaga<OrderState>(context);
                    });
                    
                    k.TopicEndpoint<BuildingBlocks.Events.Shipping.Cancelled>("saga.envio.cancelado", "saga-envio-group", e =>
                    {
                        e.UseMessageRetry(retryConfig => retryConfig.Interval(3, TimeSpan.FromSeconds(5)));
                        e.ConfigureSaga<OrderState>(context);
                    });
                    #endregion
                });
            });
            #endif
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
        
        ArgumentException.ThrowIfNullOrEmpty(settings.Endpoints.ShippingSubmitted);
        ArgumentException.ThrowIfNullOrEmpty(settings.Endpoints.ShippingAccepted);
        ArgumentException.ThrowIfNullOrEmpty(settings.Endpoints.ShippingCancelled);
        ArgumentException.ThrowIfNullOrEmpty(settings.Endpoints.ShippingRollback);
        
        return settings;
    }
}