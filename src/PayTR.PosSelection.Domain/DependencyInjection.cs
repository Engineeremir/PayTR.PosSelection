using Microsoft.Extensions.DependencyInjection;
using PayTR.PosSelection.Domain.AggregateModels.PosRatioAggregate.Services;

namespace PayTR.PosSelection.Domain;

public static class DependencyInjection
{
    public static IServiceCollection AddDomain(this IServiceCollection services)
    {
        services.AddTransient<IPosCostCalculator, PosCostCalculator>();
        return services;
    }
}
