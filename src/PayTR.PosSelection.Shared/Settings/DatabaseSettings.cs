namespace PayTR.PosSelection.Shared.Settings;

public class RedisSettings
{
    public required List<string> SentinelEndPoints { get; set; }
    public required string AbsoluteExpiration { get; set; }
    public required string SlidingExpiration { get; set; }
    public required string MasterName { get; set; }
    public required bool UseSentinel { get; set; }
    public required string ConnectionString { get; set; }
}
