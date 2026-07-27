// <copyright file="FeatureFlagService.cs" company="Defra">
// Copyright (c) Defra. All rights reserved.
// </copyright>

namespace Defra.Livestock.Sdk.Api.FeatureFlags;

using Defra.Livestock.Sdk.Api.FeatureFlags.Internal;

public class FeatureFlagService : IFeatureFlagService
{
    private readonly ICachingService cachingService;

    internal FeatureFlagService(ICachingService cachingService)
    {
        this.cachingService = cachingService;
    }
}
