#define RABBITMQ

using System.Reflection;
using MassTransit;
using MassTransit.EntityFrameworkCoreIntegration;
using Microsoft.EntityFrameworkCore;
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
                cfg.Host(settings.Host, settings.Port, "/", h =>
                {
                    h.Username(settings.Username);
                    h.Password(settings.Password);
                });

                #region Payment

                cfg.ReceiveEndpoint(settings.Endpoints.PaymentSubmitted, e =>
                {
                    e.Bind<BuildingBlocks.Events.Payment.Submitted>();
                    e.UseMessageRetry(retryConfig => retryConfig.Interval(3, TimeSpan.FromSeconds(5)));
                    e.ConfigureSaga<OrderState>(context);
                });
                
                cfg.ReceiveEndpoint(settings.Endpoints.PaymentAccepted, e =>
                {
                    e.Bind<BuildingBlocks.Events.Payment.Accepted>();
                    e.UseMessageRetry(retryConfig => retryConfig.Interval(3, TimeSpan.FromSeconds(5)));
                    e.ConfigureSaga<OrderState>(context);
                });

                cfg.ReceiveEndpoint(settings.Endpoints.PaymentCancelled, e =>
                {
                    e.Bind<BuildingBlocks.Events.Payment.Cancelled>();
                    e.UseMessageRetry(retryConfig => retryConfig.Interval(3, TimeSpan.FromSeconds(5)));
                    e.ConfigureSaga<OrderState>(context);
                });
                
                cfg.ReceiveEndpoint(settings.Endpoints.PaymentRollback, e =>
                {
                    e.Bind<BuildingBlocks.Events.Payment.Rollback>();
                    e.UseMessageRetry(retryConfig => retryConfig.Interval(3, TimeSpan.FromSeconds(5)));
                    e.ConfigureSaga<OrderState>(context);
                });

                #endregion

                #region Shipping

                cfg.ReceiveEndpoint(settings.Endpoints.ShippingSubmitted, e =>
                {
                    e.Bind<BuildingBlocks.Events.Shipping.Submitted>();
                    e.UseMessageRetry(retryConfig => retryConfig.Interval(3, TimeSpan.FromSeconds(5)));
                    e.ConfigureSaga<OrderState>(context);
                });
                
                cfg.ReceiveEndpoint(settings.Endpoints.ShippingAccepted, e =>
                {
                    e.Bind<BuildingBlocks.Events.Shipping.Accepted>();
                    e.UseMessageRetry(retryConfig => retryConfig.Interval(3, TimeSpan.FromSeconds(5)));
                    e.ConfigureSaga<OrderState>(context);
                });

                cfg.ReceiveEndpoint(settings.Endpoints.ShippingCancelled, e =>
                {
                    e.Bind<BuildingBlocks.Events.Shipping.Cancelled>();
                    e.UseMessageRetry(retryConfig => retryConfig.Interval(3, TimeSpan.FromSeconds(5)));
                    e.ConfigureSaga<OrderState>(context);
                });
                
                cfg.ReceiveEndpoint(settings.Endpoints.ShippingRollback, e =>
                {
                    e.Bind<BuildingBlocks.Events.Shipping.Rollback>();
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