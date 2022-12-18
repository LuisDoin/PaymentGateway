using CKOBankSimulator.Model;
using CKOBankSimulator.Services.Interfaces;
using Microsoft.Extensions.Caching.Distributed;

namespace CKOBankSimulator.Services
{
    public class CashingService : ICachingService
    {
        private readonly IDistributedCache _cache;
        private readonly DistributedCacheEntryOptions _options;

        public CashingService(IDistributedCache cache)
        {
            _cache = cache;
            _options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(3600),
            };
        }

        public async Task<bool> Contains(string key)
        {
            return await _cache.GetAsync(key) != null;
        }

        public async Task<string> GetAsync(string key)
        {
            return await _cache.GetStringAsync(key);
        }

        public async Task SetAsync(string key, string value)
        {
            await _cache.SetStringAsync(key, value, _options);
        }
    }
}
