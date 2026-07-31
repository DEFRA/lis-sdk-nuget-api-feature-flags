// <copyright file="FeatureFlagStatus.cs" company="Defra">
// Copyright (c) Defra. All rights reserved.
// </copyright>

namespace Defra.Livestock.Sdk.Api.FeatureFlags.Models;

public class FeatureFlagStatus
{
    public required string FlagName { get; init; }

    public required bool FlagEnabled { get; init; }

    public required bool Success { get; init; }
}
