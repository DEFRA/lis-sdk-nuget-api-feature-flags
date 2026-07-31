// <copyright file="FeatureFlagOptions.cs" company="Defra">
// Copyright (c) Defra. All rights reserved.
// </copyright>

namespace Defra.Livestock.Sdk.Api.FeatureFlags.Configuration;

public class FeatureFlagOptions
{
    private const int DefaultCacheTimeoutSeconds = 1800;

    public required string ApiKey { get; set; }

    public required string BaseUrl { get; set; }

    public required string EnvironmentName { get; set; }

    public required string ProductName { get; set; }

    public int CacheTimeoutSeconds { get; set; } = DefaultCacheTimeoutSeconds;

    // ReSharper disable once ConvertClosureToMethodGroup
    internal Func<Guid> CorrelationIdValueFactory { get; private set; } = () => Guid.NewGuid();

    public FeatureFlagOptions WithCorrelationId(Func<Guid> valueFactory)
    {
        CorrelationIdValueFactory = valueFactory ?? throw new ArgumentNullException(nameof(valueFactory));
        return this;
    }
}
