using Microsoft.Extensions.Options;
using PayTR.PosSelection.Shared.Settings;
using StackExchange.Redis;
using System.Net;
using System.Text.Json;

namespace PayTR.PosSelection.Shared.DistributedCache.Redis;

public class RedisDistributedCacheService(IOptions<RedisSettings> redisOptions, ConnectionMultiplexer connectionMultiplexer) : IRedisDistributedCacheService
{
    private async Task<ConnectionMultiplexer> ConnectToMasterAsync()
    {
        if (connectionMultiplexer == null || !connectionMultiplexer.IsConnected)
        {
            if (redisOptions.Value.UseSentinel == true)
            {
                await ConnectWithSentinel();
            }
            else
            {
                await ConnectWithoutSentinel();
            }
        }
        return connectionMultiplexer;
    }

    private async Task ConnectWithoutSentinel()
    {
        var connectionString = redisOptions.Value.ConnectionString;
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException("ConnectionString is required when not using Sentinel");
        }

        connectionMultiplexer = await ConnectionMultiplexer.ConnectAsync(connectionString);
    }

    private async Task ConnectWithSentinel()
    {
        var sentinelEndPoints = redisOptions.Value.GetType().GetProperty("SentinelEndPoints")?.GetValue(redisOptions.Value) as List<string>;
        var masterName = redisOptions.Value.GetType().GetProperty("MasterName")?.GetValue(redisOptions.Value) as string;

        if (sentinelEndPoints == null || masterName == null)
        {
            throw new InvalidOperationException("SentinelEndPoints and MasterName are required when using Sentinel");
        }

        ConfigurationOptions sentinelOptions = new()
        {
            EndPoints = new EndPointCollection(
                sentinelEndPoints
                    .Where(endpoint => !string.IsNullOrWhiteSpace(endpoint))
                    .Select(endpoint =>
                    {
                        var parts = endpoint.Split(':');
                        if (parts.Length != 2)
                        {
                            throw new FormatException($"Invalid endpoint format: {endpoint}");
                        }

                        string ipAddress = parts[0];
                        int port;

                        if (!IPAddress.TryParse(ipAddress, out var ip))
                        {
                            ip = Dns.GetHostAddresses(ipAddress).FirstOrDefault();
                            if (ip == null)
                            {
                                throw new FormatException($"Invalid IP address: {ipAddress}");
                            }
                        }

                        if (!int.TryParse(parts[1], out port))
                        {
                            throw new FormatException($"Invalid port number: {parts[1]}");
                        }

                        return new IPEndPoint(ip, port);
                    })
                    .ToArray()
            ),
            CommandMap = CommandMap.Sentinel,
            AbortOnConnectFail = false,
        };

        ConnectionMultiplexer sentinelConnection = await ConnectionMultiplexer.SentinelConnectAsync(sentinelOptions);
        EndPoint masterEndPoint = null;

        foreach (EndPoint endpoint in sentinelConnection.GetEndPoints())
        {
            IServer server = sentinelConnection.GetServer(endpoint);
            if (!server.IsConnected)
                continue;

            masterEndPoint = await server.SentinelGetMasterAddressByNameAsync(masterName);
            break;
        }

        var localMasterIP = masterEndPoint.ToString() switch
        {
            "172.25.0.2:6379" => "localhost:6379",
            "172.25.0.3:6379" => "localhost:6380",
            "172.25.0.4:6379" => "localhost:6381",
            "172.25.0.5:6379" => "localhost:6382",
        };

        connectionMultiplexer = await ConnectionMultiplexer.ConnectAsync(localMasterIP);
    }

    private async Task<IDatabase> GetDatabaseAsync(int db = 0)
    {
        var connection = await ConnectToMasterAsync();
        return connection.GetDatabase(db);
    }

    public async Task SetStringAsync(CacheRequestModel<string> request, CancellationToken cancellationToken = default)
    {
        var database = await GetDatabaseAsync((int)request.Db!);
        await database.StringSetAsync(request.Key, request.Value, request.Options.SlidingExpiration);
    }

    public async Task SetAsync<T>(CacheRequestModel<T> request, CancellationToken cancellationToken = default)
    {
        var database = await GetDatabaseAsync((int)request.Db!);
        var hashFields = new HashEntry[]
        {
       new HashEntry("data", JsonSerializer.Serialize(request.Value)),
       new HashEntry("slidingExpiration", request.Options.SlidingExpiration.HasValue ? request.Options.SlidingExpiration.Value.Ticks.ToString() : ""),
       new HashEntry("absoluteExpiration", request.Options.AbsoluteExpiration.HasValue ? request.Options.AbsoluteExpiration.Value.Ticks.ToString() : ""),
       new HashEntry("lastAccessed", DateTime.UtcNow.Ticks.ToString())
        };
        await database.HashSetAsync(request.Key, hashFields);
        if (request.Options.AbsoluteExpiration.HasValue)
        {
            var expiry = request.Options.AbsoluteExpiration.Value - DateTime.UtcNow;
            if (expiry > TimeSpan.Zero)
            {
                await database.KeyExpireAsync(request.Key, expiry);
            }
        }
    }

    public async Task<string?> GetStringAsync(CacheRequestModel<string> request, CancellationToken cancellationToken = default)
    {
        var database = await GetDatabaseAsync((int)request.Db!);
        var value = await database.StringGetAsync(request.Key);

        return value;
    }

    public async Task<T?> GetAsync<T>(CacheRequestModel<T> request, CancellationToken cancellationToken = default)
    {
        var database = await GetDatabaseAsync((int)request.Db!);
        var hashFields = await database.HashGetAllAsync(request.Key);

        if (hashFields.Length == 0)
        {
            return default;
        }

        var fieldMap = hashFields.ToDictionary(hf => hf.Name.ToString(), hf => hf.Value.ToString());

        if (fieldMap.TryGetValue("absoluteExpiration", out var absExpStr) && !string.IsNullOrEmpty(absExpStr))
        {
            var absoluteExpiration = new DateTime(long.Parse(absExpStr));

            if (DateTime.UtcNow >= absoluteExpiration)
            {
                await database.KeyDeleteAsync(request.Key);

                return default;
            }
        }

        if (fieldMap.TryGetValue("slidingExpiration", out var slidExpStr) && !string.IsNullOrEmpty(slidExpStr))
        {
            var slidingExpiration = TimeSpan.FromTicks(long.Parse(slidExpStr));
            var lastAccessed = new DateTime(long.Parse(fieldMap["lastAccessed"]));

            if (DateTime.UtcNow - lastAccessed >= slidingExpiration)
            {
                await database.KeyDeleteAsync(request.Key);

                return default;
            }

            await database.HashSetAsync(request.Key, "lastAccessed", DateTime.UtcNow.Ticks.ToString());
        }

        return JsonSerializer.Deserialize<T>(fieldMap["data"]);
    }

    public async Task DeleteAsync(CacheRequestModel<string> request, CancellationToken cancellationToken = default)
    {
        var database = await GetDatabaseAsync((int)request.Db!);
        await database.KeyDeleteAsync(request.Key);
    }

    public async Task<IEnumerable<string>> GetKeysByPatternAsync(CacheRequestModel<string> request)
    {
        var server = (await ConnectToMasterAsync()).GetServer(connectionMultiplexer.GetEndPoints().First());
        return server.Keys((int)request.Db!, request.Pattern).Select(key => key.ToString());
    }

    public async Task<long> DeleteByPatternAsync(CacheRequestModel<string> request)
    {
        var server = (await ConnectToMasterAsync()).GetServer(connectionMultiplexer.GetEndPoints().First());
        var keys = server.Keys((int)request.Db!, request.Pattern).ToArray();
        var database = await GetDatabaseAsync((int)request.Db!);
        var deleted = 0;

        foreach (var key in keys)
        {
            if (await database.KeyDeleteAsync(key))
                deleted++;
        }

        return deleted;
    }

    public async Task<long> ListLeftPushAsync<T>(CacheRequestModel<T> request)
    {
        var database = await GetDatabaseAsync((int)request.Db!);
        string jsonValue = JsonSerializer.Serialize(request.Value);
        return await database.ListLeftPushAsync(request.Key, jsonValue);
    }

    public async Task<T?> ListLeftPopAsync<T>(CacheRequestModel<string> request)
    {
        var database = await GetDatabaseAsync((int)request.Db!);
        var value = await database.ListLeftPopAsync(request.Key);

        if (value.IsNull) return default;

        return JsonSerializer.Deserialize<T>(value);
    }

    public async Task<IEnumerable<T?>> ListRangeAsync<T>(CacheRequestModel<string> request, long start, long stop)
    {
        var database = await GetDatabaseAsync((int)request.Db!);
        var values = await database.ListRangeAsync(request.Key, start, stop);

        return values
            .Where(x => !x.IsNull)
            .Select(x => JsonSerializer.Deserialize<T>(x));
    }

    public async Task<bool> SetAddAsync<T>(CacheRequestModel<string> request)
    {
        var database = await GetDatabaseAsync((int)request.Db!);
        string jsonValue = JsonSerializer.Serialize(request.Value);
        return await database.SetAddAsync(request.Key, jsonValue);
    }

    public async Task<bool> SetRemoveAsync<T>(CacheRequestModel<string> request)
    {
        var database = await GetDatabaseAsync((int)request.Db!);
        string jsonValue = JsonSerializer.Serialize(request.Value);
        return await database.SetRemoveAsync(request.Key, jsonValue);
    }

    public async Task<bool> SetContainsAsync<T>(CacheRequestModel<string> request)
    {
        var database = await GetDatabaseAsync((int)request.Db!);
        string jsonValue = JsonSerializer.Serialize(request.Value);
        return await database.SetContainsAsync(request.Key, jsonValue);
    }

    public async Task<bool> SortedSetAddAsync<T>(string key, T member, double score, int db = 0)
    {
        var database = await GetDatabaseAsync(db);
        string jsonValue = JsonSerializer.Serialize(member);
        return await database.SortedSetAddAsync(key, jsonValue, score);
    }

    public async Task<IEnumerable<T?>> SortedSetRangeByScoreAsync<T>(string key, double min, double max, int db = 0)
    {
        var database = await GetDatabaseAsync(db);
        var values = await database.SortedSetRangeByScoreAsync(key, min, max);

        return values
            .Where(x => !x.IsNull)
            .Select(x => JsonSerializer.Deserialize<T>(x!));
    }

    public async Task SetManyAsync<T>(IDictionary<string, T> keyValuePairs, CacheRequestModel<T> request, CancellationToken cancellationToken = default)
    {
        var database = await GetDatabaseAsync((int)request.Db!);
        var tasks = new List<Task>();
        var transaction = database.CreateTransaction();

        foreach (var kvp in keyValuePairs)
        {
            string jsonValue = JsonSerializer.Serialize(kvp.Value);
            tasks.Add(transaction.StringSetAsync(kvp.Key, jsonValue));
        }

        var committed = await transaction.ExecuteAsync();
        if (!committed)
        {
            throw new ApplicationException("Transaction failed to commit");
        }

        await Task.WhenAll(tasks);
    }

    public async Task<IDictionary<string, T>> GetManyAsync<T>(IEnumerable<string> keys, CacheRequestModel<string> request, CancellationToken cancellationToken = default)
    {
        var database = await GetDatabaseAsync((int)request.Db!);
        var redisKeys = keys.Select(k => (RedisKey)k).ToArray();
        var values = await database.StringGetAsync(redisKeys);

        var result = new Dictionary<string, T>();
        for (int i = 0; i < redisKeys.Length; i++)
        {
            if (!values[i].IsNull)
            {
                result[redisKeys[i]] = JsonSerializer.Deserialize<T>(values[i]);
            }
        }

        return result;
    }

    public async Task<bool> IsConnectedAsync()
    {
        try
        {
            var connection = await ConnectToMasterAsync();
            return connection.IsConnected;
        }
        catch
        {
            return false;
        }
    }

    public async Task<RedisStatus> GetServerStatusAsync()
    {
        try
        {
            var connection = await ConnectToMasterAsync();
            if (connection.IsConnected)
                return RedisStatus.Connected;
            if (connection.IsConnecting)
                return RedisStatus.Connecting;
            return RedisStatus.Disconnected;
        }
        catch
        {
            return RedisStatus.Error;
        }
    }

    public async Task<IDictionary<string, string>> GetServerInfoAsync()
    {
        var server = (await ConnectToMasterAsync()).GetServer(connectionMultiplexer.GetEndPoints().First());
        var info = await server.InfoAsync();

        return info
            .SelectMany(section => section.Select(entry => new KeyValuePair<string, string>($"{section.Key}:{entry.Key}", entry.Value)))
            .ToDictionary(x => x.Key, x => x.Value);
    }
}