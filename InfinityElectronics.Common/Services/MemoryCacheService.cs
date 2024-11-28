using InfinityElectronics.Common.Services.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace InfinityElectronics.Common.Services
{
    // Additional implementation, if the solution with Redis does not work, will be expensive, etc.
    public class MemoryCacheService : ICacheService
    {
        private readonly IMemoryCache _memoryCache;

        public MemoryCacheService(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        public Task<T> GetAsync<T>(string key)
        {
            if (_memoryCache.TryGetValue(key, out T value))
            {
                return Task.FromResult(value);
            }
            return Task.FromResult(default(T));
        }

        public Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
        {
            var options = new MemoryCacheEntryOptions();
            if (expiry.HasValue)
            {
                options.SetAbsoluteExpiration(expiry.Value);
            }

            _memoryCache.Set(key, value, options);
            return Task.CompletedTask;
        }

        public Task RemoveAsync(string key)
        {
            _memoryCache.Remove(key);
            return Task.CompletedTask;
        }
    }
}