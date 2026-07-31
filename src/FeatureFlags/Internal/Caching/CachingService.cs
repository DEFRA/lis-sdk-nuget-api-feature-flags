// <copyright file="CachingService.cs" company="Defra">
// Copyright (c) Defra. All rights reserved.
// </copyright>

namespace Defra.Livestock.Sdk.Api.FeatureFlags.Internal.Caching;

using Defra.Livestock.Sdk.Api.FeatureFlags.Configuration;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

internal class CachingService : ICachingService
{
    private readonly IMemoryCache memoryCache;
    private readonly IOptions<FeatureFlagOptions> options;

    public CachingService(IMemoryCache memoryCache, IOptions<FeatureFlagOptions> options)
    {
        this.memoryCache = memoryCache;
        this.options = options;
    }

    public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> valueFactory)
    {
        var enrichedCacheKey = $"cache_{options.Value.ProductName}_{options.Value.EnvironmentName}_{key}";
        var cacheTimeoutSeconds = this.options.Value.CacheTimeoutSeconds;

        if (memoryCache.TryGetValue(enrichedCacheKey, out T? value))
        {
            return Equals(value, default(T))
                ? throw new InvalidOperationException("Cached value returned default value")
                : value!;
        }

        value = await valueFactory();

        if (Equals(value, default(T)))
        {
            throw new InvalidOperationException("Value factory returned default value");
        }

        memoryCache.Set(enrichedCacheKey, value, TimeSpan.FromSeconds(cacheTimeoutSeconds));

        return value;
    }
}
