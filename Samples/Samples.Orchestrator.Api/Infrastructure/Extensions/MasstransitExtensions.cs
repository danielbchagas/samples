using System.Reflection;
using MassTransit;
using MassTransit.EntityFrameworkCoreIntegration;
using Microsoft.EntityFrameworkCore;
using Samples.Orchestrator.Api.Infrastructure.Database;
using Samples.Orchestrator.Api.Infrastructure.StateMachine;

namespace Samples.Orchestrator.Api.Infrastructure.Extensions;

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
            cfg.UsingInMemory((context, cfg) =>
            {
                cfg.ConfigureEndpoints(context); 
            });
            
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
            
            cfg.AddRider(rider =>
            {
                rider.AddSagaStateMachine<OrderStateMachine, OrderState>();
                
                #region Payment

                rider.AddProducer<Domain.Events.Payment.Submitted>("saga.pagamento.iniciar");
                rider.AddProducer<Domain.Events.Payment.Accepted>("saga.pagamento.confirmado");
                rider.AddProducer<Domain.Events.Payment.Cancelled>("saga.pagamento.cancelado");
                rider.AddProducer<Domain.Events.Payment.Rollback>("saga.pagamento.rollback");

                #endregion

                #region Shipping

                rider.AddProducer<Domain.Events.Shipping.Submitted>("saga.envio.iniciar");
                rider.AddProducer<Domain.Events.Shipping.Accepted>("saga.envio.confirmado");
                rider.AddProducer<Domain.Events.Shipping.Cancelled>("saga.envio.cancelado");
                rider.AddProducer<Domain.Events.Shipping.Rollback>("saga.envio.rollback");

                #endregion
                
                rider.UsingKafka((context, k) =>
                {
                    k.Host("127.0.0.1:9092");
                    
                    #region Payment
                    
                    k.TopicEndpoint<Domain.Events.Payment.Submitted>("saga.pagamento.iniciar", "saga-pagamento-group", e =>
                    {
                        e.ConfigureSaga<OrderState>(context);
                    });
                    
                    k.TopicEndpoint<Domain.Events.Payment.Accepted>("saga.pagamento.confirmado", "saga-pagamento-group", e =>
                    {
                        e.ConfigureSaga<OrderState>(context);
                    });
                    
                    k.TopicEndpoint<Domain.Events.Payment.Cancelled>("saga.pagamento.cancelado", "saga-pagamento-group", e =>
                    {
                        e.ConfigureSaga<OrderState>(context);
                    });
                    
                    k.TopicEndpoint<Domain.Events.Payment.Rollback>("saga.pagamento.rollback", "saga-pagamento-group", e =>
                    {
                        e.ConfigureSaga<OrderState>(context);
                    });
                    
                    #endregion

                    #region Shipping

                    k.TopicEndpoint<Domain.Events.Shipping.Submitted>("saga.envio.iniciar", "saga-envio-group", e =>
                    {
                        e.ConfigureSaga<OrderState>(context);
                    });
                    
                    k.TopicEndpoint<Domain.Events.Shipping.Accepted>("saga.envio.confirmado", "saga-envio-group", e =>
                    {
                        e.ConfigureSaga<OrderState>(context);
                    });
                    
                    k.TopicEndpoint<Domain.Events.Shipping.Cancelled>("saga.envio.cancelado", "saga-envio-group", e =>
                    {
                        e.ConfigureSaga<OrderState>(context);
                    });
                    
                    k.TopicEndpoint<Domain.Events.Shipping.Rollback>("saga.envio.rollback", "saga-envio-group", e =>
                    {
                        e.ConfigureSaga<OrderState>(context);
                    });

                    #endregion
                    
                });
            });
        });
            
        return services;
    }
}