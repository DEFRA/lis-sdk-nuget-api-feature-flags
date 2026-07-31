// <copyright file="CachingServiceTests.cs" company="Defra">
// Copyright (c) Defra. All rights reserved.
// </copyright>

namespace Defra.Livestock.Sdk.Api.FeatureFlags.Tests.Internal.Caching;

using Defra.Livestock.Sdk.Api.FeatureFlags.Configuration;
using Defra.Livestock.Sdk.Api.FeatureFlags.Internal.Caching;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using NSubstitute;
using Shouldly;
using Xunit;

public class CachingServiceTests
{
    private readonly IMemoryCache memoryCache;
    private readonly CachingService cachingService;
    private readonly FeatureFlagOptions optionsValue;

    public CachingServiceTests()
    {
        memoryCache = Substitute.For<IMemoryCache>();
        var options = Substitute.For<IOptions<FeatureFlagOptions>>();

        optionsValue = new FeatureFlagOptions
        {
            BaseUrl = "https://example.com",
            ApiKey = "key",
            EnvironmentName = "env",
            ProductName = "prod",
        };

        options.Value.Returns(optionsValue);

        cachingService = new CachingService(memoryCache, options);
    }

    [Fact]
    public void Constructor_ShouldInjectDependencies()
    {
        // Assert
        cachingService.ShouldNotBeNull();
    }

    [Fact]
    public async Task GetOrSetAsync_ShouldUseDefaultTimeout_WhenNotExplicitlySet()
    {
        // Arrange
        const string key = "test-key";
        const string value = "test-value";

        // Act
        await cachingService.GetOrSetAsync(key, () => Task.FromResult(value));

        // Assert
        optionsValue.CacheTimeoutSeconds.ShouldBe(1800);
        memoryCache.Received(1)
            .CreateEntry($"cache_{optionsValue.ProductName}_{optionsValue.EnvironmentName}_{key}");
    }

    [Fact]
    public async Task GetOrSetAsync_ShouldReturnCachedValue_WhenAvailable()
    {
        // Arrange
        const string key = "test-key";
        const string value = "cached-value";

        var enrichedKey = $"cache_{optionsValue.ProductName}_{optionsValue.EnvironmentName}_{key}";

        memoryCache.TryGetValue(enrichedKey, out Arg.Any<string?>())
            .Returns(x =>
            {
                x[1] = value;
                return true;
            });

        var factoryCalled = false;

        // Act
        var result = await cachingService.GetOrSetAsync(key, () =>
        {
            factoryCalled = true;
            return Task.FromResult<string?>("new-value");
        });

        // Assert
        result.ShouldBe(value);
        factoryCalled.ShouldBeFalse();
    }

    [Fact]
    public async Task GetOrSetAsync_ShouldCallFactoryAndCacheValue_WhenNotAvailable()
    {
        // Arrange
        const string key = "test-key";
        const string value = "new-value";

        var enrichedKey = $"cache_{optionsValue.ProductName}_{optionsValue.EnvironmentName}_{key}";

        memoryCache.TryGetValue(enrichedKey, out Arg.Any<string?>())
            .Returns(false);

        var factoryCalled = false;

        // Act
        var result = await cachingService.GetOrSetAsync(key, () =>
        {
            factoryCalled = true;
            return Task.FromResult<string?>(value);
        });

        // Assert
        result.ShouldBe(value);
        factoryCalled.ShouldBeTrue();
        memoryCache.Received(1).CreateEntry(enrichedKey);
    }
}
