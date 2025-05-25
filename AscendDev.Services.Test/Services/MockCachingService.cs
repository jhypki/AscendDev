using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using AscendDev.Core.Interfaces.Services;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;

namespace AscendDev.Services.Test.Services
{
    /// <summary>
    /// A mock implementation of ICachingService for testing purposes.
    /// This implementation uses a dictionary to store cached values in memory.
    /// </summary>
    public class MockCachingService : ICachingService
    {
        private readonly Dictionary<string, string> _cache = new Dictionary<string, string>();

        public async Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null)
        {
            if (_cache.TryGetValue(key, out var cachedValue))
            {
                return JsonConvert.DeserializeObject<T>(cachedValue);
            }

            var result = await factory();

            if (result != null)
            {
                var serializedResult = JsonConvert.SerializeObject(result);
                _cache[key] = serializedResult;
            }

            return result;
        }

        public Task RemoveAsync(string key)
        {
            if (_cache.ContainsKey(key))
            {
                _cache.Remove(key);
            }

            return Task.CompletedTask;
        }

        public Task<bool> ExistsAsync(string key)
        {
            return Task.FromResult(_cache.ContainsKey(key));
        }

        // Helper methods for testing
        public void SetValue<T>(string key, T value)
        {
            _cache[key] = JsonConvert.SerializeObject(value);
        }

        public void Clear()
        {
            _cache.Clear();
        }
    }
}