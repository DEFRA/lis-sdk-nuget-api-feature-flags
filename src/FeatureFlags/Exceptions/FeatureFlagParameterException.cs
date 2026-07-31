// <copyright file="FeatureFlagParameterException.cs" company="Defra">
// Copyright (c) Defra. All rights reserved.
// </copyright>

namespace Defra.Livestock.Sdk.Api.FeatureFlags.Exceptions;

public sealed class FeatureFlagParameterException : Exception
{
    public FeatureFlagParameterException(string message)
        : base(message)
    {
    }
}
