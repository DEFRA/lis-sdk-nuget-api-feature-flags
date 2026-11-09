// <copyright file="FeatureFlagHttpClient.logger.cs" company="Defra">
// Copyright (c) Defra. All rights reserved.
// </copyright>

namespace Defra.Livestock.Sdk.Api.FeatureFlags.Internal.Client;

using Microsoft.Extensions.Logging;

/// <summary>
/// Logging operations for the feature flag http client.
/// </summary>
internal partial class FeatureFlagHttpClient
{
    [LoggerMessage(
        LogLevel.Debug,
        "Sending request to Feature Flag API. GroupName: {GroupName}")]
    private static partial void LogSendingGroupRequest(ILogger logger, string groupName);

    [LoggerMessage(
        LogLevel.Debug,
        "Sending request to Feature Flag API. GroupName: {GroupName}, FlagName: {FlagName}")]
    private static partial void LogSendingFlagRequest(ILogger logger, string groupName, string flagName);

    [LoggerMessage(
        LogLevel.Warning,
        "Feature Flag API returned status code {StatusCode}. GroupName: {GroupName}")]
    private static partial void LogGroupRequestFailed(ILogger logger, int statusCode, string groupName);

    [LoggerMessage(
        LogLevel.Warning,
        "Feature Flag API returned status code {StatusCode}. GroupName: {GroupName}, FlagName: {FlagName}")]
    private static partial void LogFlagRequestFailed(ILogger logger, int statusCode, string groupName, string flagName);
}
