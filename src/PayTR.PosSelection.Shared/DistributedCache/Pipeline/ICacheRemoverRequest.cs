namespace PayTR.PosSelection.Shared.DistributedCache.Pipeline;

public interface ICacheRemoverRequest
{
    bool BypassCache { get; }
    string CacheKey { get; }
}