#define RABBITMQ

using MassTransit;
using RabbitMQ.Client;
using Samples.Orchestrator.BuildingBlocks.Events.Shipping;
using RabbitMqBusFactoryConfiguratorExtensions = MassTransit.RabbitMqBusFactoryConfiguratorExtensions;
using RabbitMqHostConfigurationExtensions = MassTransit.RabbitMqHostConfigurationExtensions;
using RoutingKeyConventionExtensions = MassTransit.RoutingKeyConventionExtensions;

namespace Samples.Orchestrator.Shipping.Infrastructure.Extensions;

public static class MasstransitExtensions
{
    public static IServiceCollection AddMasstransit(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMassTransit(cfg =>
        {
            #if RABBITMQ
            RabbitMqBusFactoryConfiguratorExtensions.UsingRabbitMq(cfg, (context, cfg) =>
            {
                RabbitMqHostConfigurationExtensions.Host(cfg, "localhost", 15672, "/", h =>
                {
                    h.Username("user");
                    h.Password("password");
                });
                
                cfg.Message<Submitted>(x => { });
                cfg.Publish<Submitted>(x => x.ExchangeType = ExchangeType.Direct);
                cfg.Send<Submitted>(x => RoutingKeyConventionExtensions.UseRoutingKeyFormatter(x, context => "saga.envio.iniciar"));
                
                cfg.Message<Rollback>(x => { });
                cfg.Publish<Rollback>(x => x.ExchangeType = ExchangeType.Direct);
                cfg.Send<Rollback>(x => RoutingKeyConventionExtensions.UseRoutingKeyFormatter(x, context => "saga.envio.rollback"));
            });
            #endif
            
            #if KAFKA
            cfg.AddRider(rider =>
            {
                rider.AddSagaStateMachine<OrderStateMachine, OrderState>();
                
                rider.AddProducer<BuildingBlocks.Events.Shipping.Submitted>("saga.envio.iniciar");
                rider.AddProducer<BuildingBlocks.Events.Shipping.Rollback>("saga.envio.rollback");
            
                rider.UsingKafka((context, k) =>
                {
                    k.Host("127.0.0.1:9092");
                    
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
                });
            });
            #endif
        });
            
        return services;
    }
}