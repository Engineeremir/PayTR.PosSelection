using PayTR.PosSelection.Shared.DistributedCache.Redis;

namespace PayTR.PosSelection.Shared.DistributedCache;

public class CacheRequestModel<T>
{
    public T Value { get; set; }
    public string StringValue { get; set; }
    public string Key { get; set; }
    public RedisDb? Db { get; set; }
    public CacheEntryOptions Options { get; set; }
    public ExpirationType ExpirationType { get; set; }
    public string Pattern { get; set; }
}