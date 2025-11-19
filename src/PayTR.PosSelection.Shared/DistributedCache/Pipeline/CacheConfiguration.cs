namespace PayTR.PosSelection.Shared.DistributedCache.Pipeline;

public class CacheConfiguration
{
    public string? ConnectionString { get; set; }
    public string? AbsoluteExpiration { get; set; }
    public string? SlidingExpiration { get; set; }
    public bool UseSentinel { get; set; }
}