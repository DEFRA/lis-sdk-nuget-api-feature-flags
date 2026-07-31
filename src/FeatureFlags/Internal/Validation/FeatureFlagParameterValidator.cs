// <copyright file="FeatureFlagParameterValidator.cs" company="Defra">
// Copyright (c) Defra. All rights reserved.
// </copyright>

namespace Defra.Livestock.Sdk.Api.FeatureFlags.Internal.Validation;

using Defra.Livestock.Sdk.Api.FeatureFlags.Exceptions;

internal static class FeatureFlagParameterValidator
{
    public static void ValidateGroupName(string groupName)
    {
        if (string.IsNullOrWhiteSpace(groupName))
        {
            throw new FeatureFlagParameterException("FeatureFlag GroupName is required");
        }

        if (groupName.Length > 100)
        {
            throw new FeatureFlagParameterException("FeatureFlag GroupName cannot exceed 100 characters");
        }
    }

    public static void ValidateFlagName(string flagName)
    {
        if (string.IsNullOrWhiteSpace(flagName))
        {
            throw new FeatureFlagParameterException("FeatureFlag FlagName is required");
        }
    }
}
