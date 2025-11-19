using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PayTR.PosSelection.Shared.SeedWork.HttpClient;

namespace PayTR.PosSelection.Shared;

public static class DependencyInjection
{
    public static IServiceCollection AddShared(this IServiceCollection services)
    {
        services.AddHttpClient();
        services.AddScoped<IBaseHttpClientService, BaseHttpClientService>();
        return services;
    }
}
