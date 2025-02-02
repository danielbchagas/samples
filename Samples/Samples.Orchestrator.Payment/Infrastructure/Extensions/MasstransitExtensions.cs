#define RABBITMQ

using MassTransit;
using RabbitMQ.Client;
using Samples.Orchestrator.BuildingBlocks.Events.Payment;

namespace Samples.Orchestrator.Payment.Infrastructure.Extensions;

public static class MasstransitExtensions
{
    public static IServiceCollection AddMasstransit(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMassTransit(cfg =>
        {
            #if RABBITMQ
            cfg.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host("localhost", 15672, "/", h =>
                {
                    h.Username("user");
                    h.Password("password");
                });
                
                cfg.Message<Submitted>(x => { });
                cfg.Publish<Submitted>(x => x.ExchangeType = ExchangeType.Direct);
                cfg.Send<Submitted>(x => x.UseRoutingKeyFormatter(context => "saga.pagamento.iniciar"));

                cfg.Message<Rollback>(x => { });
                cfg.Publish<Rollback>(x => x.ExchangeType = ExchangeType.Direct);
                cfg.Send<Rollback>(x => x.UseRoutingKeyFormatter(context => "saga.pagamento.rollback"));
            });
            #endif
            
            #if KAFKA
            cfg.AddRider(rider =>
            {
                rider.AddSagaStateMachine<OrderStateMachine, OrderState>();
            
                rider.AddProducer<BuildingBlocks.Events.Payment.Submitted>("saga.pagamento.iniciar");
                rider.AddProducer<BuildingBlocks.Events.Payment.Rollback>("saga.pagamento.rollback");
            
                rider.UsingKafka((context, k) =>
                {
                    k.Host("127.0.0.1:9092");
                    
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
                });
            });
            #endif
        });
            
        return services;
    }
}