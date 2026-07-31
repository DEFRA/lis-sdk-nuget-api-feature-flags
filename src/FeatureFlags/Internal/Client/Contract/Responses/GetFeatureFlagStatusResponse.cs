// <copyright file="GetFeatureFlagStatusResponse.cs" company="Defra">
// Copyright (c) Defra. All rights reserved.
// </copyright>

namespace Defra.Livestock.Sdk.Api.FeatureFlags.Internal.Client.Contract.Responses;

using Defra.Livestock.Sdk.Api.FeatureFlags.Internal.Client.Contract.Responses.Common;

internal class GetFeatureFlagStatusResponse : FeatureFlagStatusResponse
{
    public required bool Success { get; init; }
}
