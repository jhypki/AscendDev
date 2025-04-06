using AscendDev.Core.Interfaces.Services;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;

namespace AscendDev.Services.Services;

public class CachingService(IDistributedCache cache) : ICachingService
{
    private readonly IDistributedCache _cache = cache ?? throw new ArgumentNullException(nameof(cache));
    private readonly TimeSpan _defaultExpiration = TimeSpan.FromMinutes(30);

    public async Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null)
    {
        var cachedValue = await _cache.GetStringAsync(key);

        if (!string.IsNullOrEmpty(cachedValue)) return JsonConvert.DeserializeObject<T>(cachedValue);

        var result = await factory();

        if (result == null) return result;
        var serializedResult = JsonConvert.SerializeObject(result);
        await _cache.SetStringAsync(
            key,
            serializedResult,
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration ?? _defaultExpiration
            });

        return result;
    }

    public async Task RemoveAsync(string key)
    {
        await _cache.RemoveAsync(key);
    }

    public async Task<bool> ExistsAsync(string key)
    {
        return await _cache.GetStringAsync(key) != null;
    }
}