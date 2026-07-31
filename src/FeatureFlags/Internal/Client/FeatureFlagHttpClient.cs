// <copyright file="FeatureFlagHttpClient.cs" company="Defra">
// Copyright (c) Defra. All rights reserved.
// </copyright>

namespace Defra.Livestock.Sdk.Api.FeatureFlags.Internal.Client;

using System.Net.Http.Json;
using Defra.Livestock.Sdk.Api.FeatureFlags.Configuration;
using Defra.Livestock.Sdk.Api.FeatureFlags.Exceptions;
using Defra.Livestock.Sdk.Api.FeatureFlags.Internal.Client.Contract.Responses;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

internal class FeatureFlagHttpClient : IFeatureFlagHttpClient
{
    private const string FailedToDeserializeResponse = "Failed to deserialize response";

    private static readonly System.Text.Json.JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true, PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.SnakeCaseLower,
    };

    private readonly HttpClient client;
    private readonly IOptions<FeatureFlagOptions> options;
    private readonly ILogger<FeatureFlagHttpClient> logger;

    public FeatureFlagHttpClient(
        HttpClient client,
        IOptions<FeatureFlagOptions> options,
        ILogger<FeatureFlagHttpClient> logger)
    {
        this.client = client;
        this.options = options;
        this.logger = logger;
    }

    public async Task<GetFeatureFlagGroupStatusResponse> GetFeatureFlagGroupStatusAsync(
        string groupName,
        CancellationToken cancellationToken = default)
    {
        var relativeUrl = $"evaluate/{Uri.EscapeDataString(groupName)}";

        logger.LogInformation(
            "Sending request to Feature Flag API. BaseUrl: {BaseUrl}, RelativeUrl: {RelativeUrl}, GroupName: {GroupName}",
            options.Value.BaseUrl,
            relativeUrl,
            groupName);

        var httpRequest = new HttpRequestMessage(HttpMethod.Get, new Uri($"{options.Value.BaseUrl}/{relativeUrl}", UriKind.Absolute));

        AddHeaders(httpRequest);

        using var httpResponse = await client.SendAsync(httpRequest, cancellationToken);

        httpResponse.EnsureSuccessStatusCode();

        return await httpResponse.Content.ReadFromJsonAsync<GetFeatureFlagGroupStatusResponse>(
                   options: JsonSerializerOptions,
                   cancellationToken: cancellationToken)
               ?? throw new InvalidOperationException(FailedToDeserializeResponse);
    }

    public async Task<GetFeatureFlagStatusResponse> GetFeatureFlagStatusAsync(
        string groupName,
        string flagName,
        CancellationToken cancellationToken = default)
    {
        var relativeUrl = $"evaluate/{Uri.EscapeDataString(groupName)}/{Uri.EscapeDataString(flagName)}";

        logger.LogInformation(
            "Sending request to Feature Flag API. BaseUrl: {BaseUrl}, RelativeUrl: {RelativeUrl}, GroupName: {GroupName}, FlagName: {FlagName}",
            options.Value.BaseUrl,
            relativeUrl,
            groupName,
            flagName);

        var httpRequest = new HttpRequestMessage(HttpMethod.Get, new Uri($"{options.Value.BaseUrl}/{relativeUrl}", UriKind.Absolute));

        AddHeaders(httpRequest);

        using var httpResponse = await client.SendAsync(httpRequest, cancellationToken);

        httpResponse.EnsureSuccessStatusCode();

        return await httpResponse.Content.ReadFromJsonAsync<GetFeatureFlagStatusResponse>(
                   options: JsonSerializerOptions,
                   cancellationToken: cancellationToken)
               ?? throw new InvalidOperationException(FailedToDeserializeResponse);
    }

    private void AddHeaders(HttpRequestMessage httpRequest)
    {
        var correlationId = options.Value.CorrelationIdValueFactory.Invoke();

        if (correlationId == Guid.Empty)
        {
            throw new FeatureFlagParameterException("FeatureFlag CorrelationId cannot be empty");
        }

        httpRequest.Headers.Add("x-api-key", options.Value.ApiKey);
        httpRequest.Headers.Add("x-cdp-request-id", correlationId.ToString());
        httpRequest.Headers.Add("product-name", options.Value.ProductName);
        httpRequest.Headers.Add("environment-name", options.Value.EnvironmentName);
    }
}
