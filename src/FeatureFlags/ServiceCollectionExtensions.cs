// <copyright file="ServiceCollectionExtensions.cs" company="Defra">
// Copyright (c) Defra. All rights reserved.
// </copyright>

namespace Defra.Livestock.Sdk.Api.FeatureFlags;

using Defra.Livestock.Sdk.Api.FeatureFlags.Internal;
using Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddFeatureFlags()
        {
            services.AddTransient<ICachingService, CachingService>();
            services.AddTransient<IFeatureFlagService, FeatureFlagService>();

            return services;
        }
    }
}
