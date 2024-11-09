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
            
            cfg.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host("localhost", "/", h =>
                {
                    h.Username("user");
                    h.Password("password");
                });
                
                cfg.ReceiveEndpoint("saga.pagamento.iniciar", e =>
                {
                    e.ConfigureSaga<OrderState>(context);
                });
                
                cfg.ReceiveEndpoint("saga.pagamento.confirmado", e =>
                {
                    e.ConfigureSaga<OrderState>(context);
                });
                
                cfg.ReceiveEndpoint("saga.pagamento.cancelado", e =>
                {
                    e.ConfigureSaga<OrderState>(context);
                });
                
                cfg.ReceiveEndpoint("saga.pagamento.rollback", e =>
                {
                    e.ConfigureSaga<OrderState>(context);
                });
                
                cfg.ReceiveEndpoint("saga.envio.iniciar", e =>
                {
                    e.ConfigureSaga<OrderState>(context);
                });
                
                cfg.ReceiveEndpoint("saga.envio.confirmado", e =>
                {
                    e.ConfigureSaga<OrderState>(context);
                });
                
                cfg.ReceiveEndpoint("saga.envio.cancelado", e =>
                {
                    e.ConfigureSaga<OrderState>(context);
                });

                cfg.ReceiveEndpoint("saga.envio.rollback", e =>
                {
                    e.ConfigureSaga<OrderState>(context);
                });
            });
        });
            
        return services;
    }
}