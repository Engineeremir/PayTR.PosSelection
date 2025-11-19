using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using PayTR.PosSelection.Shared.DistributedCache.Pipeline;
using PayTR.PosSelection.Shared.DistributedCache.Redis;
using PayTR.PosSelection.Shared.Settings;
using StackExchange.Redis;

namespace PayTR.PosSelection.Shared.DistributedCache
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddCaching(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<RedisSettings>(configuration.GetSection("VaultSecrets:RedisSettings"));

            services.AddSingleton<ICacheService, RedisDistributedCacheService>();
            services.AddSingleton<IRedisDistributedCacheService, RedisDistributedCacheService>();
            
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(CachingBehavior<,>));
            services.AddSingleton<ConnectionMultiplexer>(provider =>
            {
                var redisSettings = provider.GetRequiredService<IOptions<RedisSettings>>().Value;

                if (redisSettings.UseSentinel)
                {
                    var sentinelConfig = new ConfigurationOptions();
                    foreach (var endpoint in redisSettings.SentinelEndPoints)
                    {
                        sentinelConfig.EndPoints.Add(endpoint);
                    }
                    sentinelConfig.ServiceName = redisSettings.MasterName;
                    return ConnectionMultiplexer.Connect(sentinelConfig);
                }
                else
                {
                    return ConnectionMultiplexer.Connect(redisSettings.ConnectionString);
                }
            });

            return services;
        }
    }
}
