using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PayTR.PosSelection.Domain.AggregateModels.PosRatioAggregate;
using PayTR.PosSelection.Infrastructure.EFCore.Contexts;
using PayTR.PosSelection.Infrastructure.EFCore.Repositories;
using PayTR.PosSelection.Shared.Utils;

namespace PayTR.PosSelection.Infrastructure.EFCore;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureEfCore(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<PosSelectionDbContext>(options =>
            options.UseNpgsql(configuration.GetSecretValue<string>("DatabaseSettings", "PosSelectionDatabase")));

        services.AddRepositories();

        var serviceProvider = services.BuildServiceProvider();
        var db = serviceProvider.GetRequiredService<PosSelectionDbContext>();
        db.Database.Migrate();

        return services;
    }

    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IPosRatioRepository, PosRatioRepository>();
        services.AddScoped(typeof(EfRepository<>));

        return services;
    }
}