// <copyright file="ServiceCollectionExtensionsTests.cs" company="Defra">
// Copyright (c) Defra. All rights reserved.
// </copyright>

namespace Defra.Livestock.Sdk.Api.FeatureFlags.Tests;

using Defra.Livestock.Sdk.Api.FeatureFlags.Configuration;
using Defra.Livestock.Sdk.Api.FeatureFlags.Internal.Caching;
using Defra.Livestock.Sdk.Api.FeatureFlags.Internal.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Shouldly;
using Xunit;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddFeatureFlags_RegistersServicesAndConfiguresHttpClientCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();
        const string expectedEndpoint = "https://featureflags.example.com/";
        const string expectedApiKey = "test-api-key-123";
        var expectedCorrelationId = Guid.NewGuid();
        const string expectedEnvironmentName = "Dev";
        const string expectedProductName = "TestProduct";
        const int expectedCacheTimeoutSeconds = 300;

        // Act
        services.AddFeatureFlags(options =>
        {
            options.BaseUrl = expectedEndpoint;
            options.ApiKey = expectedApiKey;
            options.EnvironmentName = expectedEnvironmentName;
            options.ProductName = expectedProductName;
            options.CacheTimeoutSeconds = expectedCacheTimeoutSeconds;
            options.WithCorrelationId(() => expectedCorrelationId);
        });

        var provider = services.BuildServiceProvider();
        var featureFlagService = provider.GetService<IFeatureFlagService>();
        var featureFlagHttpClient = provider.GetService<IFeatureFlagHttpClient>();
        var cachingService = provider.GetService<ICachingService>();
        var optionsValue = provider.GetService<IOptions<FeatureFlagOptions>>()?.Value;

        // Assert
        featureFlagService.ShouldNotBeNull();
        featureFlagHttpClient.ShouldNotBeNull();
        cachingService.ShouldNotBeNull();
        optionsValue.ShouldNotBeNull();

        optionsValue.BaseUrl.ShouldBe(expectedEndpoint);
        optionsValue.ApiKey.ShouldBe(expectedApiKey);
        optionsValue.EnvironmentName.ShouldBe(expectedEnvironmentName);
        optionsValue.ProductName.ShouldBe(expectedProductName);
        optionsValue.CacheTimeoutSeconds.ShouldBe(expectedCacheTimeoutSeconds);
    }

    [Fact]
    public void WithCorrelationId_ShouldThrowArgumentNullException_WhenValueFactoryIsNull()
    {
        // Arrange
        var options = new FeatureFlagOptions
        {
            ApiKey = "key", BaseUrl = "https://example.com", EnvironmentName = "env", ProductName = "prod",
        };

        // Act
        var exception = Record.Exception(() => options.WithCorrelationId(null!));

        // Assert
        exception.ShouldBeOfType<ArgumentNullException>();
    }

    [Fact]
    public void AddFeatureFlags_WithDefaultOptions_RegistersServices()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddFeatureFlags(options =>
        {
            options.BaseUrl = "https://featureflags.example.com/";
            options.ApiKey = "test-api-key";
            options.EnvironmentName = "Dev";
            options.ProductName = "TestProduct";
        });

        var provider = services.BuildServiceProvider();
        var featureFlagService = provider.GetService<IFeatureFlagService>();
        var optionsValue = provider.GetService<IOptions<FeatureFlagOptions>>()?.Value;

        // Assert
        featureFlagService.ShouldNotBeNull();
        optionsValue.ShouldNotBeNull();
        optionsValue.CacheTimeoutSeconds.ShouldBe(1800);
        optionsValue.CorrelationIdValueFactory.ShouldNotBeNull();
        optionsValue.CorrelationIdValueFactory().ShouldNotBe(Guid.Empty);
    }

    [Fact]
    public void AddFeatureFlags_WithInvalidOptions_ShouldThrowOptionsValidationException_OnStart()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddFeatureFlags(options =>
        {
            options.ApiKey = string.Empty;
            options.BaseUrl = "https://featureflags.example.com/";
            options.EnvironmentName = "Dev";
            options.ProductName = "TestProduct";
        });

        var provider = services.BuildServiceProvider();

        // Act
        var exception = Record.Exception(() => provider.GetRequiredService<IOptions<FeatureFlagOptions>>().Value);

        // Assert
        exception.ShouldBeOfType<OptionsValidationException>();
        var validationException = (OptionsValidationException)exception;
        validationException.Failures.ShouldContain("FeatureFlag ApiKey is required");
    }
}
