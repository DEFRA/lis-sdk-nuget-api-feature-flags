// <copyright file="FeatureFlagStatusResponse.cs" company="Defra">
// Copyright (c) Defra. All rights reserved.
// </copyright>

namespace Defra.Livestock.Sdk.Api.FeatureFlags.Internal.Client.Contract.Responses.Common;

internal class FeatureFlagStatusResponse
{
    public required string FlagName { get; init; }

    public bool FlagEnabled { get; init; }
}
