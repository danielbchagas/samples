using Microsoft.EntityFrameworkCore;
using Samples.Orchestrator.Payment.Domain.Order;

namespace Samples.Orchestrator.Payment.Infrastructure.Database;

public class PaymentDbContext : DbContext
{
    public PaymentDbContext(DbContextOptions<PaymentDbContext> options) : base(options)
    {
        
    }
    
    public DbSet<Order> Orders { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new OrderMap());
    }
}