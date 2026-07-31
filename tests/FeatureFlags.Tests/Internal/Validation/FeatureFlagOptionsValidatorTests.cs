// <copyright file="FeatureFlagOptionsValidatorTests.cs" company="Defra">
// Copyright (c) Defra. All rights reserved.
// </copyright>

namespace Defra.Livestock.Sdk.Api.FeatureFlags.Tests.Internal.Validation;

using Defra.Livestock.Sdk.Api.FeatureFlags.Configuration;
using Defra.Livestock.Sdk.Api.FeatureFlags.Internal.Validation;
using Shouldly;
using Xunit;

public class FeatureFlagOptionsValidatorTests
{
    private readonly FeatureFlagOptionsValidator validator = new();

    [Fact]
    public void Validate_ShouldReturnSuccess_WhenOptionsAreValid()
    {
        // Arrange
        var options = new FeatureFlagOptions
        {
            ApiKey = "test", BaseUrl = "https://example.com", EnvironmentName = "Dev", ProductName = "Product",
        };

        // Act
        var result = validator.Validate(null, options);

        // Assert
        result.Succeeded.ShouldBeTrue();
    }

    [Theory]
#pragma warning disable xUnit1012
    [InlineData(null)]
#pragma warning restore xUnit1012
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_ShouldReturnFail_WhenApiKeyIsMissing(string apiKey)
    {
        // Arrange
        var options = new FeatureFlagOptions
        {
            ApiKey = apiKey, BaseUrl = "https://example.com", EnvironmentName = "Dev", ProductName = "Product",
        };

        // Act
        var result = validator.Validate(null, options);

        // Assert
        result.Failed.ShouldBeTrue();
        result.FailureMessage.ShouldContain("FeatureFlag ApiKey is required");
    }

    [Fact]
    public void Validate_ShouldReturnFail_WhenApiKeyIsTooLong()
    {
        // Arrange
        var options = new FeatureFlagOptions
        {
            ApiKey = new string('a', 101),
            BaseUrl = "https://example.com",
            EnvironmentName = "Dev",
            ProductName = "Product",
        };

        // Act
        var result = validator.Validate(null, options);

        // Assert
        result.Failed.ShouldBeTrue();
        result.FailureMessage.ShouldContain("FeatureFlag ApiKey cannot exceed 100 characters");
    }

    [Theory]
#pragma warning disable xUnit1012
    [InlineData(null)]
#pragma warning restore xUnit1012
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_ShouldReturnFail_WhenBaseUrlIsMissing(string baseUrl)
    {
        // Arrange
        var options = new FeatureFlagOptions
        {
            ApiKey = "key", BaseUrl = baseUrl, EnvironmentName = "Dev", ProductName = "Product",
        };

        // Act
        var result = validator.Validate(null, options);

        // Assert
        result.Failed.ShouldBeTrue();
        result.FailureMessage.ShouldContain("FeatureFlag BaseUrl is required");
    }

    [Theory]
    [InlineData("not-a-url")]
    [InlineData("https://")]
    [InlineData("example.com")]
    public void Validate_ShouldReturnFail_WhenBaseUrlIsInvalid(string baseUrl)
    {
        // Arrange
        var options = new FeatureFlagOptions
        {
            ApiKey = "key", BaseUrl = baseUrl, EnvironmentName = "Dev", ProductName = "Product",
        };

        // Act
        var result = validator.Validate(null, options);

        // Assert
        result.Failed.ShouldBeTrue();
        result.FailureMessage.ShouldContain("FeatureFlag BaseUrl is not a valid URL");
    }

    [Fact]
    public void Validate_ShouldReturnFail_WhenBaseUrlIsTooLong()
    {
        // Arrange
        var options = new FeatureFlagOptions
        {
            ApiKey = "key", BaseUrl = new string('a', 2001), EnvironmentName = "Dev", ProductName = "Product",
        };

        // Act
        var result = validator.Validate(null, options);

        // Assert
        result.Failed.ShouldBeTrue();
        result.FailureMessage.ShouldContain("FeatureFlag BaseUrl cannot exceed 2000 characters");
    }

    [Theory]
#pragma warning disable xUnit1012
    [InlineData(null)]
#pragma warning restore xUnit1012
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_ShouldReturnFail_WhenEnvironmentNameIsMissing(string environmentName)
    {
        // Arrange
        var options = new FeatureFlagOptions
        {
            ApiKey = "key", BaseUrl = "https://example.com", EnvironmentName = environmentName, ProductName = "Product",
        };

        // Act
        var result = validator.Validate(null, options);

        // Assert
        result.Failed.ShouldBeTrue();
        result.FailureMessage.ShouldContain("FeatureFlag EnvironmentName is required");
    }

    [Fact]
    public void Validate_ShouldReturnFail_WhenEnvironmentNameIsTooLong()
    {
        // Arrange
        var options = new FeatureFlagOptions
        {
            ApiKey = "key",
            BaseUrl = "https://example.com",
            EnvironmentName = new string('a', 11),
            ProductName = "Product",
        };

        // Act
        var result = validator.Validate(null, options);

        // Assert
        result.Failed.ShouldBeTrue();
        result.FailureMessage.ShouldContain("FeatureFlag EnvironmentName cannot exceed 10 characters");
    }

    [Theory]
#pragma warning disable xUnit1012
    [InlineData(null)]
#pragma warning restore xUnit1012
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_ShouldReturnFail_WhenProductNameIsMissing(string productName)
    {
        // Arrange
        var options = new FeatureFlagOptions
        {
            ApiKey = "key", BaseUrl = "https://example.com", EnvironmentName = "Dev", ProductName = productName,
        };

        // Act
        var result = validator.Validate(null, options);

        // Assert
        result.Failed.ShouldBeTrue();
        result.FailureMessage.ShouldContain("FeatureFlag ProductName is required");
    }

    [Fact]
    public void Validate_ShouldReturnFail_WhenProductNameIsTooLong()
    {
        // Arrange
        var options = new FeatureFlagOptions
        {
            ApiKey = "key", BaseUrl = "https://example.com", EnvironmentName = "Dev", ProductName = new string('a', 51),
        };

        // Act
        var result = validator.Validate(null, options);

        // Assert
        result.Failed.ShouldBeTrue();
        result.FailureMessage.ShouldContain("FeatureFlag ProductName cannot exceed 50 characters");
    }

    [Theory]
    [InlineData(29)]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_ShouldReturnFail_WhenCacheTimeoutSecondsIsTooSmall(int timeout)
    {
        // Arrange
        var options = new FeatureFlagOptions
        {
            ApiKey = "key",
            BaseUrl = "https://example.com",
            EnvironmentName = "Dev",
            ProductName = "Product",
            CacheTimeoutSeconds = timeout,
        };

        // Act
        var result = validator.Validate(null, options);

        // Assert
        result.Failed.ShouldBeTrue();
        result.FailureMessage.ShouldContain("FeatureFlag CacheTimeoutSeconds must be 30 or greater");
    }

    [Fact]
    public void Validate_ShouldReturnFail_WhenCacheTimeoutSecondsIsTooLarge()
    {
        // Arrange
        var options = new FeatureFlagOptions
        {
            ApiKey = "key",
            BaseUrl = "https://example.com",
            EnvironmentName = "Dev",
            ProductName = "Product",
            CacheTimeoutSeconds = 86401,
        };

        // Act
        var result = validator.Validate(null, options);

        // Assert
        result.Failed.ShouldBeTrue();
        result.FailureMessage.ShouldContain("FeatureFlag CacheTimeoutSeconds cannot exceed 86400");
    }

    [Theory]
    [InlineData(30)]
    [InlineData(3600)]
    [InlineData(86400)]
    public void Validate_ShouldReturnSuccess_WhenCacheTimeoutSecondsIsValid(int timeout)
    {
        // Arrange
        var options = new FeatureFlagOptions
        {
            ApiKey = "key",
            BaseUrl = "https://example.com",
            EnvironmentName = "Dev",
            ProductName = "Product",
            CacheTimeoutSeconds = timeout,
        };

        // Act
        var result = validator.Validate(null, options);

        // Assert
        result.Succeeded.ShouldBeTrue();
    }
}
