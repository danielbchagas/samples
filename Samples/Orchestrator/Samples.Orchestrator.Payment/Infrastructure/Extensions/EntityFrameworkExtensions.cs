using Microsoft.EntityFrameworkCore;
using Samples.Orchestrator.Payment.Infrastructure.Database;

namespace Samples.Orchestrator.Payment.Infrastructure.Extensions;

public static class EntityFrameworkExtensions
{
    public static void AddEntityFramework(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<DbContext, PaymentDbContext>(opt =>
        {
            opt.UseNpgsql(configuration.GetConnectionString("DefaultConnection"), options => options.EnableRetryOnFailure());
        });
    }
}