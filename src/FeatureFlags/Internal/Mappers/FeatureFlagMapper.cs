// <copyright file="FeatureFlagMapper.cs" company="Defra">
// Copyright (c) Defra. All rights reserved.
// </copyright>

namespace Defra.Livestock.Sdk.Api.FeatureFlags.Internal.Mappers;

using Defra.Livestock.Sdk.Api.FeatureFlags.Internal.Client.Contract.Responses;
using Defra.Livestock.Sdk.Api.FeatureFlags.Models;

internal static class FeatureFlagMapper
{
    public static FeatureFlagGroupStatus MapFromGetFeatureFlagGroupStatusResponse(
        GetFeatureFlagGroupStatusResponse response)
    {
        return new FeatureFlagGroupStatus
        {
            GroupName = response.GroupName,
            GroupEnabled = response.GroupEnabled,
            Success = response.Success,
            Features =
            [
                .. response.Features.Select(feature => new FeatureFlagStatus()
                {
                    FlagName = feature.FlagName, FlagEnabled = feature.FlagEnabled, Success = true,
                })
            ],
        };
    }

    public static FeatureFlagStatus MapFromGetFeatureFlagStatusResponse(
        GetFeatureFlagStatusResponse response)
    {
        return new FeatureFlagStatus
        {
            FlagName = response.FlagName, FlagEnabled = response.FlagEnabled, Success = response.Success,
        };
    }
}
