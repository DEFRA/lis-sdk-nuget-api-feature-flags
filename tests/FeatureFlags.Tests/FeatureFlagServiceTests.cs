// <copyright file="FeatureFlagServiceTests.cs" company="Defra">
// Copyright (c) Defra. All rights reserved.
// </copyright>

namespace Defra.Livestock.Sdk.Api.FeatureFlags.Tests;

using Defra.Livestock.Sdk.Api.FeatureFlags.Exceptions;
using Defra.Livestock.Sdk.Api.FeatureFlags.Internal.Caching;
using Defra.Livestock.Sdk.Api.FeatureFlags.Internal.Client;
using Defra.Livestock.Sdk.Api.FeatureFlags.Internal.Client.Contract.Responses;
using Defra.Livestock.Sdk.Api.FeatureFlags.Models;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Shouldly;
using Xunit;

public class FeatureFlagServiceTests
{
    private readonly ICachingService cachingService;
    private readonly IFeatureFlagHttpClient featureFlagHttpClient;
    private readonly FeatureFlagService featureFlagService;

    public FeatureFlagServiceTests()
    {
        cachingService = Substitute.For<ICachingService>();
        featureFlagHttpClient = Substitute.For<IFeatureFlagHttpClient>();
        featureFlagService = new FeatureFlagService(cachingService, featureFlagHttpClient);
    }

    [Fact]
    public async Task GetFeatureFlagStatusAsync_ShouldReturnCachedResponse_WhenAvailable()
    {
        // Arrange
        const string groupName = "TestGroup";
        const string flagName = "TestFlag";

        var cachedStatus = new FeatureFlagStatus { FlagName = flagName, FlagEnabled = true, Success = true };

        this.cachingService.GetOrSetAsync(
                $"flag_{groupName}_{flagName}",
                Arg.Any<Func<Task<FeatureFlagStatus>>>())
            .Returns(cachedStatus);

        // Act
        var result =
            await featureFlagService.GetFeatureFlagStatusAsync(
                groupName,
                flagName,
                TestContext.Current.CancellationToken);

        // Assert
        result.ShouldBe(cachedStatus);

        await featureFlagHttpClient.DidNotReceiveWithAnyArgs()
            .GetFeatureFlagStatusAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetFeatureFlagStatusAsync_ShouldCallHttpClientAndCacheResponse_WhenNotCached()
    {
        // Arrange
        const string groupName = "TestGroup";
        const string flagName = "TestFlag";

        var clientResponse = new GetFeatureFlagStatusResponse
        {
            FlagName = flagName, FlagEnabled = true, Success = true,
        };

        cachingService.GetOrSetAsync(
                $"flag_{groupName}_{flagName}",
                Arg.Any<Func<Task<FeatureFlagStatus>>>())
            .Returns(async (info) =>
            {
                var factory = info.Arg<Func<Task<FeatureFlagStatus>>>();
                return await factory();
            });

        featureFlagHttpClient.GetFeatureFlagStatusAsync(groupName, flagName, Arg.Any<CancellationToken>())
            .Returns(clientResponse);

        // Act
        var result =
            await featureFlagService.GetFeatureFlagStatusAsync(
                groupName,
                flagName,
                TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.FlagName.ShouldBe(flagName);

        await featureFlagHttpClient.Received(1)
            .GetFeatureFlagStatusAsync(groupName, flagName, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetFeatureFlagGroupStatusAsync_ShouldReturnCachedResponse_WhenAvailable()
    {
        // Arrange
        const string groupName = "TestGroup";

        var cachedStatus =
            new FeatureFlagGroupStatus { GroupName = groupName, GroupEnabled = true, Success = true, Features = [] };

        cachingService.GetOrSetAsync(
                $"group_{groupName}",
                Arg.Any<Func<Task<FeatureFlagGroupStatus>>>())
            .Returns(cachedStatus);

        // Act
        var result =
            await featureFlagService.GetFeatureFlagGroupStatusAsync(
                groupName,
                TestContext.Current.CancellationToken);

        // Assert
        result.ShouldBe(cachedStatus);

        await featureFlagHttpClient.DidNotReceiveWithAnyArgs()
            .GetFeatureFlagGroupStatusAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetFeatureFlagGroupStatusAsync_ShouldCallHttpClientAndCacheResponse_WhenNotCached()
    {
        // Arrange
        const string groupName = "TestGroup";

        var clientResponse = new GetFeatureFlagGroupStatusResponse
        {
            GroupName = groupName, GroupEnabled = true, Success = true, Features = [],
        };

        cachingService.GetOrSetAsync(
                $"group_{groupName}",
                Arg.Any<Func<Task<FeatureFlagGroupStatus>>>())
            .Returns(async (info) =>
            {
                var factory = info.Arg<Func<Task<FeatureFlagGroupStatus>>>();
                return await factory();
            });

        featureFlagHttpClient.GetFeatureFlagGroupStatusAsync(groupName, Arg.Any<CancellationToken>())
            .Returns(clientResponse);

        // Act
        var result =
            await featureFlagService.GetFeatureFlagGroupStatusAsync(
                groupName,
                TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.GroupName.ShouldBe(groupName);

        await featureFlagHttpClient.Received(1)
            .GetFeatureFlagGroupStatusAsync(groupName, Arg.Any<CancellationToken>());
    }

    [Theory]
#pragma warning disable xUnit1012
    [InlineData(null, "FeatureFlag GroupName is required")]
#pragma warning restore xUnit1012
    [InlineData("", "FeatureFlag GroupName is required")]
    [InlineData("   ", "FeatureFlag GroupName is required")]
    public async Task GetFeatureFlagGroupStatusAsync_ShouldThrowFeatureFlagParameterException_WhenGroupNameIsInvalid(string groupName, string expectedMessage)
    {
        // Act
        var exception = await Record.ExceptionAsync(() =>
            featureFlagService.GetFeatureFlagGroupStatusAsync(groupName, TestContext.Current.CancellationToken));

        // Assert
        exception.ShouldBeOfType<FeatureFlagParameterException>();
        exception.Message.ShouldBe(expectedMessage);
    }

    [Fact]
    public async Task GetFeatureFlagGroupStatusAsync_ShouldThrowFeatureFlagParameterException_WhenGroupNameIsTooLong()
    {
        // Arrange
        var groupName = new string('a', 101);

        // Act
        var exception = await Record.ExceptionAsync(() =>
            featureFlagService.GetFeatureFlagGroupStatusAsync(groupName, TestContext.Current.CancellationToken));

        // Assert
        exception.ShouldBeOfType<FeatureFlagParameterException>();
        exception.Message.ShouldBe("FeatureFlag GroupName cannot exceed 100 characters");
    }

    [Fact]
    public async Task GetFeatureFlagGroupStatusAsync_ShouldWrapExceptionInFeatureFlagQueryException()
    {
        // Arrange
        const string groupName = "TestGroup";
        var exceptionMessage = "Network error";
        var originalException = new Exception(exceptionMessage);

        cachingService.GetOrSetAsync(
                $"group_{groupName}",
                Arg.Any<Func<Task<FeatureFlagGroupStatus>>>())
            .Throws(originalException);

        // Act
        var exception = await Record.ExceptionAsync(() =>
            featureFlagService.GetFeatureFlagGroupStatusAsync(groupName, TestContext.Current.CancellationToken));

        // Assert
        exception.ShouldBeOfType<FeatureFlagQueryException>();
        exception.Message.ShouldBe($"Failed to get feature flag group status for group '{groupName}':'{exceptionMessage}'");
    }

    [Fact]
    public async Task GetFeatureFlagGroupStatusAsync_ShouldRethrowFeatureFlagQueryException()
    {
        // Arrange
        const string groupName = "TestGroup";
        var expectedMessage = "Query error";
        var originalException = new FeatureFlagQueryException(expectedMessage);

        cachingService.GetOrSetAsync(
                $"group_{groupName}",
                Arg.Any<Func<Task<FeatureFlagGroupStatus>>>())
            .Throws(originalException);

        // Act
        var exception = await Record.ExceptionAsync(() =>
            featureFlagService.GetFeatureFlagGroupStatusAsync(groupName, TestContext.Current.CancellationToken));

        // Assert
        exception.ShouldBe(originalException);
    }

    [Theory]
#pragma warning disable xUnit1012
    [InlineData(null, "TestFlag", "FeatureFlag GroupName is required")]
#pragma warning restore xUnit1012
    [InlineData("", "TestFlag", "FeatureFlag GroupName is required")]
    [InlineData("   ", "TestFlag", "FeatureFlag GroupName is required")]
#pragma warning disable xUnit1012
    [InlineData("TestGroup", null, "FeatureFlag FlagName is required")]
#pragma warning restore xUnit1012
    [InlineData("TestGroup", "", "FeatureFlag FlagName is required")]
    [InlineData("TestGroup", "   ", "FeatureFlag FlagName is required")]
    public async Task GetFeatureFlagStatusAsync_ShouldThrowFeatureFlagParameterException_WhenArgumentsAreInvalid(string groupName, string flagName, string expectedMessage)
    {
        // Act
        var exception = await Record.ExceptionAsync(() =>
            featureFlagService.GetFeatureFlagStatusAsync(groupName, flagName, TestContext.Current.CancellationToken));

        // Assert
        exception.ShouldBeOfType<FeatureFlagParameterException>();
        exception.Message.ShouldBe(expectedMessage);
    }

    [Fact]
    public async Task GetFeatureFlagStatusAsync_ShouldThrowFeatureFlagParameterException_WhenGroupNameIsTooLong()
    {
        // Arrange
        var groupName = new string('a', 101);
        const string flagName = "TestFlag";

        // Act
        var exception = await Record.ExceptionAsync(() =>
            featureFlagService.GetFeatureFlagStatusAsync(groupName, flagName, TestContext.Current.CancellationToken));

        // Assert
        exception.ShouldBeOfType<FeatureFlagParameterException>();
        exception.Message.ShouldBe("FeatureFlag GroupName cannot exceed 100 characters");
    }

    [Fact]
    public async Task GetFeatureFlagStatusAsync_ShouldWrapExceptionInFeatureFlagQueryException()
    {
        // Arrange
        const string groupName = "TestGroup";
        const string flagName = "TestFlag";
        var exceptionMessage = "Network error";
        var originalException = new Exception(exceptionMessage);

        cachingService.GetOrSetAsync(
                $"flag_{groupName}_{flagName}",
                Arg.Any<Func<Task<FeatureFlagStatus>>>())
            .Throws(originalException);

        // Act
        var exception = await Record.ExceptionAsync(() =>
            featureFlagService.GetFeatureFlagStatusAsync(groupName, flagName, TestContext.Current.CancellationToken));

        // Assert
        exception.ShouldBeOfType<FeatureFlagQueryException>();
        exception.Message.ShouldBe($"Failed to get feature flag status for flag '{flagName}' within group '{groupName}': '{exceptionMessage}'");
    }

    [Fact]
    public async Task GetFeatureFlagStatusAsync_ShouldRethrowFeatureFlagQueryException()
    {
        // Arrange
        const string groupName = "TestGroup";
        const string flagName = "TestFlag";
        var expectedMessage = "Query error";
        var originalException = new FeatureFlagQueryException(expectedMessage);

        cachingService.GetOrSetAsync(
                $"flag_{groupName}_{flagName}",
                Arg.Any<Func<Task<FeatureFlagStatus>>>())
            .Throws(originalException);

        // Act
        var exception = await Record.ExceptionAsync(() =>
            featureFlagService.GetFeatureFlagStatusAsync(groupName, flagName, TestContext.Current.CancellationToken));

        // Assert
        exception.ShouldBe(originalException);
    }
}
