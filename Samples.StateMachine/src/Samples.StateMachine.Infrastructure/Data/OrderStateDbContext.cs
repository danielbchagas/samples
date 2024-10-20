using System.Reflection;
using MassTransit.EntityFrameworkCoreIntegration;
using Microsoft.EntityFrameworkCore;
using Samples.StateMachine.Infrastructure.Configurations;
using Samples.StateMachine.Infrastructure.Services.BasicMachine;

namespace Samples.StateMachine.Infrastructure.Data;

public class OrderStateDbContext(DbContextOptions<OrderStateDbContext> options) : SagaDbContext(options)
{
    public DbSet<OrderState> States { get; set; }
    
    protected override IEnumerable<ISagaClassMap> Configurations
    {
        get { yield return new OrderStateMap(); }
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}