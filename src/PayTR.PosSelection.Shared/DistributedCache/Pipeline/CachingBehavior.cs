using PayTR.PosSelection.Shared.DistributedCache;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace PayTR.PosSelection.Shared.DistributedCache.Pipeline;

public class CachingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
   where TRequest : IRequest<TResponse>, ICachableRequest
{
    private readonly ICacheService _cacheService;
    private readonly ILogger<CachingBehavior<TRequest, TResponse>> _logger;
    private readonly CacheConfiguration _cacheOptions;

    public CachingBehavior(ICacheService cacheService, ILogger<CachingBehavior<TRequest, TResponse>> logger, IConfiguration configuration)
    {
        _cacheService = cacheService;
        _logger = logger;
        _cacheOptions = configuration.GetSection("Redis").Get<CacheConfiguration>();
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var cacheKey = typeof(TRequest).Name;

        CacheRequestModel<string> cacheRequestModel = new()
        {
            Db = Redis.RedisDb.DB1,
            Key = cacheKey
        };

        var cachedResponse = await _cacheService.GetStringAsync(cacheRequestModel, cancellationToken);

        if (cachedResponse != null)
        {
            var response = JsonSerializer.Deserialize<TResponse>(cachedResponse);
            _logger.LogInformation($"Fetched from Cache -> {cacheKey}");

            return response;
        }

        var result = await next();

        var options = new CacheEntryOptions
        {
            SlidingExpiration = TimeSpan.FromMinutes(Convert.ToInt32(_cacheOptions.SlidingExpiration!)),
            AbsoluteExpiration = DateTime.UtcNow.AddMinutes(Convert.ToInt32(_cacheOptions.AbsoluteExpiration!)),
        };

        var serializedData = JsonSerializer.Serialize(result);
        cacheRequestModel.Options = options;
        cacheRequestModel.Value = serializedData;

        await _cacheService.SetStringAsync(cacheRequestModel, cancellationToken);
        _logger.LogInformation($"Added to Cache -> {cacheKey}");

        return result;
    }
}