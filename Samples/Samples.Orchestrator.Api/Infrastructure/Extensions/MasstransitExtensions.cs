#define RABBITMQ

using System.Reflection;
using MassTransit;
using MassTransit.EntityFrameworkCoreIntegration;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using Samples.Orchestrator.Api.Infrastructure.Database;
using Samples.Orchestrator.Api.Infrastructure.StateMachine;

namespace Samples.Orchestrator.Api.Infrastructure.Extensions;

/*
 * https://masstransit.io/
 * https://masstransit.io/documentation/configuration/transports/rabbitmq
 * https://masstransit.io/documentation/transports/rabbitmq
 */

public static class MasstransitExtensions
{
    public static IServiceCollection AddMasstransit(this IServiceCollection services, IConfiguration configuration)
    {
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
                cfg.Host("localhost", 15672, "/", h =>
                {
                    h.Username("user");
                    h.Password("password");
                });
                
                cfg.Message<BuildingBlocks.Events.Payment.Submitted>(x => { });
                cfg.Publish<BuildingBlocks.Events.Payment.Submitted>(x => x.ExchangeType = ExchangeType.Direct);
                cfg.Send<BuildingBlocks.Events.Payment.Submitted>(x => x.UseRoutingKeyFormatter(context => "saga.pagamento.iniciar"));

                cfg.Message<BuildingBlocks.Events.Payment.Rollback>(x => { });
                cfg.Publish<BuildingBlocks.Events.Payment.Rollback>(x => x.ExchangeType = ExchangeType.Direct);
                cfg.Send<BuildingBlocks.Events.Payment.Rollback>(x => x.UseRoutingKeyFormatter(context => "saga.pagamento.rollback"));
                
                cfg.Message<BuildingBlocks.Events.Shipping.Submitted>(x => { });
                cfg.Publish<BuildingBlocks.Events.Shipping.Submitted>(x => x.ExchangeType = ExchangeType.Direct);
                cfg.Send<BuildingBlocks.Events.Shipping.Submitted>(x => x.UseRoutingKeyFormatter(context => "saga.envio.iniciar"));
                
                cfg.Message<BuildingBlocks.Events.Shipping.Rollback>(x => { });
                cfg.Publish<BuildingBlocks.Events.Shipping.Rollback>(x => x.ExchangeType = ExchangeType.Direct);
                cfg.Send<BuildingBlocks.Events.Shipping.Rollback>(x => x.UseRoutingKeyFormatter(context => "saga.envio.rollback"));

                cfg.ReceiveEndpoint("saga.pagamento.confirmado", e =>
                {
                    e.Bind<BuildingBlocks.Events.Payment.Accepted>();
                    e.UseMessageRetry(retryConfig => retryConfig.Interval(3, TimeSpan.FromSeconds(5)));
                    e.ConfigureSaga<OrderState>(context);
                });

                cfg.ReceiveEndpoint("saga.pagamento.cancelado", e =>
                {
                    e.Bind<BuildingBlocks.Events.Payment.Cancelled>();
                    e.UseMessageRetry(retryConfig => retryConfig.Interval(3, TimeSpan.FromSeconds(5)));
                    e.ConfigureSaga<OrderState>(context);
                });

                cfg.ReceiveEndpoint("saga.envio.confirmado", e =>
                {
                    e.Bind<BuildingBlocks.Events.Shipping.Accepted>();
                    e.UseMessageRetry(retryConfig => retryConfig.Interval(3, TimeSpan.FromSeconds(5)));
                    e.ConfigureSaga<OrderState>(context);
                });

                cfg.ReceiveEndpoint("saga.envio.cancelado", e =>
                {
                    e.Bind<BuildingBlocks.Events.Shipping.Cancelled>();
                    e.UseMessageRetry(retryConfig => retryConfig.Interval(3, TimeSpan.FromSeconds(5)));
                    e.ConfigureSaga<OrderState>(context);
                });
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
}