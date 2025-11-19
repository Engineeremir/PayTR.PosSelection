using PayTR.PosSelection.Shared.DistributedCache;
using StackExchange.Redis;

namespace PayTR.PosSelection.Shared.DistributedCache.Redis;

public interface IRedisDistributedCacheService : ICacheService
{
    Task<IEnumerable<string>> GetKeysByPatternAsync(CacheRequestModel<string> request);
    Task<long> DeleteByPatternAsync(CacheRequestModel<string> request);
    Task<long> ListLeftPushAsync<T>(CacheRequestModel<T> request);
    Task<T?> ListLeftPopAsync<T>(CacheRequestModel<string> request);
    Task<IEnumerable<T?>> ListRangeAsync<T>(CacheRequestModel<string> request, long start, long stop);
    Task<bool> SetRemoveAsync<T>(CacheRequestModel<string> request);
    Task<bool> SetContainsAsync<T>(CacheRequestModel<string> request);
    Task<bool> SortedSetAddAsync<T>(string key, T member, double score, int db = 0);
    Task<IEnumerable<T?>> SortedSetRangeByScoreAsync<T>(string key, double min, double max, int db = 0);
    Task<bool> IsConnectedAsync();
    Task<RedisStatus> GetServerStatusAsync();
    Task<IDictionary<string, string>> GetServerInfoAsync();

}