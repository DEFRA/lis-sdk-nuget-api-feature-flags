// <copyright file="IFeatureFlagHttpClient.cs" company="Defra">
// Copyright (c) Defra. All rights reserved.
// </copyright>

namespace Defra.Livestock.Sdk.Api.FeatureFlags.Internal.Client;

using Defra.Livestock.Sdk.Api.FeatureFlags.Internal.Client.Contract.Responses;

internal interface IFeatureFlagHttpClient
{
    Task<GetFeatureFlagGroupStatusResponse> GetFeatureFlagGroupStatusAsync(
        string groupName,
        CancellationToken cancellationToken = default);

    Task<GetFeatureFlagStatusResponse> GetFeatureFlagStatusAsync(
        string groupName,
        string flagName,
        CancellationToken cancellationToken = default);
}
