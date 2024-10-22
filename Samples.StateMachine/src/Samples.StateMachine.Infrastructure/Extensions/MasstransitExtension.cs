using System.Reflection;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Samples.StateMachine.Infrastructure.Consumers;
using Samples.StateMachine.Infrastructure.Data;
using Samples.StateMachine.Infrastructure.Services.BasicMachine;

namespace Samples.StateMachine.Infrastructure.Extensions;

public static class MasstransitExtension
{
    public static IServiceCollection AddMasstransit(this IServiceCollection services)
    {
        services.AddDbContext<OrderStateDbContext>(options =>
            options.UseNpgsql("User ID=postgres;Password=mysecretpassword;Host=localhost;Port=5432;Database=state_machine;Pooling=true;"));
        
        services.AddMassTransit(cfg =>
        {
            cfg.AddConsumer<OrderSubmittedConsumer>();
            
            cfg.AddSagaStateMachine<OrderStateMachine, OrderState>()
                .EntityFrameworkRepository(r =>
                {
                    r.ConcurrencyMode = ConcurrencyMode.Pessimistic;
                    
                    r.AddDbContext<DbContext, OrderStateDbContext>((provider, builder) =>
                    {
                        builder.UseNpgsql("User ID=postgres;Password=mysecretpassword;Host=localhost;Port=5432;Database=state_machine;Pooling=true;",
                            npgsqlOptions =>
                            {
                                npgsqlOptions.MigrationsAssembly(Assembly.GetExecutingAssembly().GetName().Name);
                                npgsqlOptions.MigrationsHistoryTable($"__{nameof(OrderStateDbContext)}");
                            });
                    });
                    
                    r.UsePostgres();
                });
                // .InMemoryRepository();
                
            cfg.UsingInMemory((context, cfg) =>
            {
                cfg.ConfigureEndpoints(context);
            });
                
            // cfg.UsingRabbitMq((context, cfg) =>
            // {
            //     cfg.Host("localhost", "/", h =>
            //     {
            //         h.Username("user");
            //         h.Password("password");
            //     });
            //
            //     cfg.ConfigureEndpoints(context);
            // });
                
            // cfg.AddRider(rider =>
            // {
            //     // rider.AddProducer<YourMessage>("topic-name");
            //
            //     rider.UsingKafka((context, k) =>
            //     {
            //         k.Host("localhost:2181");
            //     });
            // });
        });

        return services;
    }
}