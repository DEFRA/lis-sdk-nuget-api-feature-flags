// <copyright file="IFeatureFlagService.cs" company="Defra">
// Copyright (c) Defra. All rights reserved.
// </copyright>

namespace Defra.Livestock.Sdk.Api.FeatureFlags;

using Defra.Livestock.Sdk.Api.FeatureFlags.Models;

public interface IFeatureFlagService
{
    /// <summary>
    /// Gets the status of all feature flags in a group.
    /// </summary>
    /// <param name="groupName">The name of the feature flag group.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A <see cref="FeatureFlagGroupStatus"/> containing the status of the group and its features.</returns>
    Task<FeatureFlagGroupStatus> GetFeatureFlagGroupStatusAsync(
        string groupName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the status of a specific feature flag.
    /// </summary>
    /// <param name="groupName">The name of the feature flag group.</param>
    /// <param name="flagName">The name of the feature flag.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A <see cref="FeatureFlagStatus"/> containing the status of the feature flag.</returns>
    Task<FeatureFlagStatus> GetFeatureFlagStatusAsync(
        string groupName,
        string flagName,
        CancellationToken cancellationToken = default);
}
