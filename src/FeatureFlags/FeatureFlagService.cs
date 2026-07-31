// <copyright file="FeatureFlagService.cs" company="Defra">
// Copyright (c) Defra. All rights reserved.
// </copyright>

namespace Defra.Livestock.Sdk.Api.FeatureFlags;

using Defra.Livestock.Sdk.Api.FeatureFlags.Exceptions;
using Defra.Livestock.Sdk.Api.FeatureFlags.Internal.Caching;
using Defra.Livestock.Sdk.Api.FeatureFlags.Internal.Client;
using Defra.Livestock.Sdk.Api.FeatureFlags.Internal.Mappers;
using Defra.Livestock.Sdk.Api.FeatureFlags.Internal.Validation;
using Defra.Livestock.Sdk.Api.FeatureFlags.Models;

public class FeatureFlagService : IFeatureFlagService
{
    private readonly ICachingService cachingService;
    private readonly IFeatureFlagHttpClient featureFlagHttpClient;

    internal FeatureFlagService(ICachingService cachingService, IFeatureFlagHttpClient featureFlagHttpClient)
    {
        this.cachingService = cachingService;
        this.featureFlagHttpClient = featureFlagHttpClient;
    }

    /// <inheritdoc/>
    public async Task<FeatureFlagGroupStatus> GetFeatureFlagGroupStatusAsync(
        string groupName,
        CancellationToken cancellationToken = default)
    {
        FeatureFlagParameterValidator.ValidateGroupName(groupName);

        try
        {
            return await cachingService.GetOrSetAsync($"group_{groupName}", async () =>
                FeatureFlagMapper.MapFromGetFeatureFlagGroupStatusResponse(
                    await featureFlagHttpClient.GetFeatureFlagGroupStatusAsync(groupName, cancellationToken)));
        }
        catch (FeatureFlagParameterException)
        {
            throw;
        }
        catch (FeatureFlagQueryException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new FeatureFlagQueryException(
                $"Failed to get feature flag group status for group '{groupName}':'{ex.Message}'");
        }
    }

    /// <inheritdoc/>
    public async Task<FeatureFlagStatus> GetFeatureFlagStatusAsync(
        string groupName,
        string flagName,
        CancellationToken cancellationToken = default)
    {
        FeatureFlagParameterValidator.ValidateGroupName(groupName);
        FeatureFlagParameterValidator.ValidateFlagName(flagName);

        try
        {
            return await cachingService.GetOrSetAsync($"flag_{groupName}_{flagName}", async () =>
                FeatureFlagMapper.MapFromGetFeatureFlagStatusResponse(
                    await featureFlagHttpClient.GetFeatureFlagStatusAsync(
                        groupName,
                        flagName,
                        cancellationToken)));
        }
        catch (FeatureFlagParameterException)
        {
            throw;
        }
        catch (FeatureFlagQueryException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new FeatureFlagQueryException(
                $"Failed to get feature flag status for flag '{flagName}' within group '{groupName}': '{ex.Message}'");
        }
    }
}
