// <copyright file="ServiceCollectionExtensions.cs" company="Defra">
// Copyright (c) Defra. All rights reserved.
// </copyright>

namespace Defra.Livestock.Sdk.Api.FeatureFlags;

using System;
using Defra.Livestock.Sdk.Api.FeatureFlags.Configuration;
using Defra.Livestock.Sdk.Api.FeatureFlags.Internal.Caching;
using Defra.Livestock.Sdk.Api.FeatureFlags.Internal.Client;
using Defra.Livestock.Sdk.Api.FeatureFlags.Internal.Validation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds the feature flag services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configureOptions">The action to configure the feature flag options.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddFeatureFlags(
        this IServiceCollection services,
        Action<FeatureFlagOptions> configureOptions)
    {
        services.Configure(configureOptions);

        services.TryAddEnumerable(
            ServiceDescriptor.Singleton<IValidateOptions<FeatureFlagOptions>, FeatureFlagOptionsValidator>());

        services.AddOptions<FeatureFlagOptions>()
            .ValidateOnStart();

        services.AddMemoryCache();
        services.AddHttpClient<IFeatureFlagHttpClient, FeatureFlagHttpClient>();
        services.AddTransient<ICachingService, CachingService>();

        services.AddTransient<IFeatureFlagService>(sp =>
            new FeatureFlagService(
                sp.GetRequiredService<ICachingService>(),
                sp.GetRequiredService<IFeatureFlagHttpClient>()));

        return services;
    }
}
