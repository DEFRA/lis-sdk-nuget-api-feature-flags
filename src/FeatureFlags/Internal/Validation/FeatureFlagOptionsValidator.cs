// <copyright file="FeatureFlagOptionsValidator.cs" company="Defra">
// Copyright (c) Defra. All rights reserved.
// </copyright>

namespace Defra.Livestock.Sdk.Api.FeatureFlags.Internal.Validation;

using Defra.Livestock.Sdk.Api.FeatureFlags.Configuration;
using Microsoft.Extensions.Options;

internal class FeatureFlagOptionsValidator : IValidateOptions<FeatureFlagOptions>
{
    public ValidateOptionsResult Validate(string? name, FeatureFlagOptions options)
    {
        var validationFailures = new List<string>();

        if (string.IsNullOrWhiteSpace(options.ApiKey))
        {
            validationFailures.Add("FeatureFlag ApiKey is required");
        }

        if (options.ApiKey?.Length > 100)
        {
            validationFailures.Add("FeatureFlag ApiKey cannot exceed 100 characters");
        }

        if (string.IsNullOrWhiteSpace(options.BaseUrl))
        {
            validationFailures.Add("FeatureFlag BaseUrl is required");
        }

        if (options.BaseUrl?.Length > 2000)
        {
            validationFailures.Add("FeatureFlag BaseUrl cannot exceed 2000 characters");
        }

        if (!Uri.TryCreate(options.BaseUrl, UriKind.Absolute, out _))
        {
            validationFailures.Add("FeatureFlag BaseUrl is not a valid URL");
        }

        if (string.IsNullOrWhiteSpace(options.EnvironmentName))
        {
            validationFailures.Add("FeatureFlag EnvironmentName is required");
        }

        if (options.EnvironmentName?.Length > 10)
        {
            validationFailures.Add("FeatureFlag EnvironmentName cannot exceed 10 characters");
        }

        if (string.IsNullOrWhiteSpace(options.ProductName))
        {
            validationFailures.Add("FeatureFlag ProductName is required");
        }

        if (options.ProductName?.Length > 50)
        {
            validationFailures.Add("FeatureFlag ProductName cannot exceed 50 characters");
        }

        switch (options.CacheTimeoutSeconds)
        {
            case < 30:
                validationFailures.Add("FeatureFlag CacheTimeoutSeconds must be 30 or greater");
                break;
            case > 86400:
                validationFailures.Add("FeatureFlag CacheTimeoutSeconds cannot exceed 86400");
                break;
        }

        return validationFailures.Count > 0
            ? ValidateOptionsResult.Fail(validationFailures)
            : ValidateOptionsResult.Success;
    }
}
