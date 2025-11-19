namespace PayTR.PosSelection.Shared.DistributedCache;

public interface ICacheService
{
    Task SetStringAsync(CacheRequestModel<string> request, CancellationToken cancellationToken = default);
    Task SetAsync<T>(CacheRequestModel<T> request, CancellationToken cancellationToken = default);
    Task<string?> GetStringAsync(CacheRequestModel<string> request, CancellationToken cancellationToken = default);
    Task<T?> GetAsync<T>(CacheRequestModel<T> request, CancellationToken cancellationToken = default);
    Task DeleteAsync(CacheRequestModel<string> request, CancellationToken cancellationToken = default);
    Task<IDictionary<string, T>> GetManyAsync<T>(IEnumerable<string> keys, CacheRequestModel<string> request, CancellationToken cancellationToken = default);
    Task SetManyAsync<T>(IDictionary<string, T> keyValuePairs, CacheRequestModel<T> request, CancellationToken cancellationToken = default);
}