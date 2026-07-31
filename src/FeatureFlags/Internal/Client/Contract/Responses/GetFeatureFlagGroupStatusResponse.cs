// <copyright file="GetFeatureFlagGroupStatusResponse.cs" company="Defra">
// Copyright (c) Defra. All rights reserved.
// </copyright>

namespace Defra.Livestock.Sdk.Api.FeatureFlags.Internal.Client.Contract.Responses;

using Defra.Livestock.Sdk.Api.FeatureFlags.Internal.Client.Contract.Responses.Common;

internal class GetFeatureFlagGroupStatusResponse
{
    public required string GroupName { get; init; }

    public bool GroupEnabled { get; init; }

    public List<FeatureFlagStatusResponse> Features { get; init; } = [];

    public bool Success { get; init; }
}
