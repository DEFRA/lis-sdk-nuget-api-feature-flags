// <copyright file="FeatureFlagQueryException.cs" company="Defra">
// Copyright (c) Defra. All rights reserved.
// </copyright>

namespace Defra.Livestock.Sdk.Api.FeatureFlags.Exceptions;

public sealed class FeatureFlagQueryException : Exception
{
    public FeatureFlagQueryException(string message)
        : base(message)
    {
    }
}
