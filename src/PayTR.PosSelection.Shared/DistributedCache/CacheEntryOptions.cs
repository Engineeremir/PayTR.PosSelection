namespace PayTR.PosSelection.Shared.DistributedCache;

public class CacheEntryOptions
{
    public TimeSpan? SlidingExpiration { get; set; }
    public DateTime? AbsoluteExpiration { get; set; }
    public TimeSpan Expiration { get; set; }
}
