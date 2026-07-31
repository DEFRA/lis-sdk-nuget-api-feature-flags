// <copyright file="FeatureFlagGroupStatus.cs" company="Defra">
// Copyright (c) Defra. All rights reserved.
// </copyright>

namespace Defra.Livestock.Sdk.Api.FeatureFlags.Models;

public class FeatureFlagGroupStatus
{
    public required string GroupName { get; init; }

    public required bool GroupEnabled { get; init; }

    public List<FeatureFlagStatus> Features { get; init; } = [];

    public required bool Success { get; init; }
}
