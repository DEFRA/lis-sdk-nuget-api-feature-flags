// <copyright file="FeatureFlagHttpClientTests.cs" company="Defra">
// Copyright (c) Defra. All rights reserved.
// </copyright>

namespace Defra.Livestock.Sdk.Api.FeatureFlags.Tests.Internal.Client;

using System.Net;
using Defra.Livestock.Sdk.Api.FeatureFlags.Configuration;
using Defra.Livestock.Sdk.Api.FeatureFlags.Exceptions;
using Defra.Livestock.Sdk.Api.FeatureFlags.Internal.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using Shouldly;
using Xunit;

public class FeatureFlagHttpClientTests
{
    private readonly IOptions<FeatureFlagOptions> options;
    private readonly ILogger<FeatureFlagHttpClient> logger;
    private readonly Guid correlationId = Guid.NewGuid();

    public FeatureFlagHttpClientTests()
    {
        options = Substitute.For<IOptions<FeatureFlagOptions>>();
        logger = Substitute.For<ILogger<FeatureFlagHttpClient>>();
        logger.IsEnabled(Arg.Any<LogLevel>()).Returns(true);

        options.Value.Returns(new FeatureFlagOptions
        {
            BaseUrl = "https://example.com/",
            ApiKey = "test-api-key",
            EnvironmentName = "Dev",
            ProductName = "TestProduct",
        });
    }

    [Fact]
    public async Task GetFeatureFlagStatusAsync_ShouldAddHeadersAndBaseAddressCorrectly()
    {
        // Arrange
        options.Value.WithCorrelationId(() => correlationId);
        var expectedCorrelationId = this.correlationId.ToString();

        var handler = new TestHttpMessageHandler(req =>
        {
            req.Headers.GetValues("x-api-key").ShouldContain("test-api-key");
            req.Headers.GetValues("x-cdp-request-id").ShouldContain(expectedCorrelationId);
            req.Headers.GetValues("product-name").ShouldContain("TestProduct");
            req.Headers.GetValues("environment-name").ShouldContain("Dev");
            req.RequestUri.ShouldNotBeNull();
            req.RequestUri.AbsoluteUri.ShouldStartWith("https://example.com/");
            req.RequestUri.AbsoluteUri.ShouldContain("evaluate/TestGroup/TestFlag");

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    """
                    {
                      "flag_name": "TestFlag",
                      "flag_enabled": true,
                      "success": true
                    }
                    """,
                    System.Text.Encoding.UTF8,
                    "application/json"),
            };
        });

        var httpClient = new HttpClient(handler);
        var featureFlagHttpClient = new FeatureFlagHttpClient(httpClient, this.options, this.logger);

        // Act
        var result =
            await featureFlagHttpClient.GetFeatureFlagStatusAsync(
                "TestGroup",
                "TestFlag",
                TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.FlagName.ShouldBe("TestFlag");
        result.FlagEnabled.ShouldBeTrue();

        logger.Received(1).Log(
            LogLevel.Debug,
            Arg.Any<EventId>(),
            Arg.Is<object>(v =>
                v.ToString() == "Sending request to Feature Flag API. GroupName: TestGroup, FlagName: TestFlag"),
            null,
            Arg.Any<Func<object, Exception?, string>>());
        AssertNothingLoggedAtOrAbove(LogLevel.Information);
    }

    [Fact]
    public async Task GetFeatureFlagStatusAsync_ShouldInvokeCorrelationIdOnEachRequest()
    {
        // Arrange
        var correlationId1 = Guid.NewGuid();
        var correlationId2 = Guid.NewGuid();
        var ids = new Queue<Guid>([correlationId1, correlationId2]);

        options.Value.Returns(new FeatureFlagOptions
        {
            BaseUrl = "https://example.com/",
            ApiKey = "test-api-key",
            EnvironmentName = "Dev",
            ProductName = "TestProduct",
        }.WithCorrelationId(ids.Dequeue));

        var callCount = 0;

        var handler = new TestHttpMessageHandler(req =>
        {
            callCount++;

            var expectedId = callCount == 1 ? correlationId1 : correlationId2;

            req.Headers.GetValues("x-cdp-request-id").ShouldContain(expectedId.ToString());

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    """
                    {
                      "flag_name": "TestFlag",
                      "flag_enabled": true,
                      "success": true
                    }
                    """,
                    System.Text.Encoding.UTF8,
                    "application/json"),
            };
        });

        var httpClient = new HttpClient(handler);
        var featureFlagHttpClient = new FeatureFlagHttpClient(httpClient, this.options, this.logger);

        // Act
        await featureFlagHttpClient.GetFeatureFlagStatusAsync("G", "F", TestContext.Current.CancellationToken);
        await featureFlagHttpClient.GetFeatureFlagStatusAsync("G", "F", TestContext.Current.CancellationToken);

        // Assert
        callCount.ShouldBe(2);
    }

    [Fact]
    public async Task GetFeatureFlagStatusAsync_ShouldThrowFeatureFlagParameterException_WhenCorrelationIdIsEmpty()
    {
        // Arrange
        options.Value.WithCorrelationId(() => Guid.Empty);

        var handler = new TestHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK));
        var httpClient = new HttpClient(handler);
        var featureFlagHttpClient = new FeatureFlagHttpClient(httpClient, this.options, this.logger);

        // Act
        var exception = await Record.ExceptionAsync(() =>
            featureFlagHttpClient.GetFeatureFlagStatusAsync("G", "F", TestContext.Current.CancellationToken));

        // Assert
        exception.ShouldBeOfType<FeatureFlagParameterException>();
        exception.Message.ShouldBe("FeatureFlag CorrelationId cannot be empty");
    }

    [Fact]
    public async Task GetFeatureFlagGroupStatusAsync_ShouldThrowFeatureFlagParameterException_WhenCorrelationIdIsEmpty()
    {
        // Arrange
        options.Value.WithCorrelationId(() => Guid.Empty);

        var handler = new TestHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK));
        var httpClient = new HttpClient(handler);
        var featureFlagHttpClient = new FeatureFlagHttpClient(httpClient, this.options, this.logger);

        // Act
        var exception = await Record.ExceptionAsync(() =>
            featureFlagHttpClient.GetFeatureFlagGroupStatusAsync("G", TestContext.Current.CancellationToken));

        // Assert
        exception.ShouldBeOfType<FeatureFlagParameterException>();
        exception.Message.ShouldBe("FeatureFlag CorrelationId cannot be empty");
    }

    [Fact]
    public async Task GetFeatureFlagGroupStatusAsync_ShouldAddHeadersAndBaseAddressCorrectly()
    {
        // Arrange
        options.Value.WithCorrelationId(() => correlationId);
        var expectedCorrelationId = this.correlationId.ToString();

        var handler = new TestHttpMessageHandler(req =>
        {
            req.Headers.GetValues("x-api-key").ShouldContain("test-api-key");
            req.Headers.GetValues("x-cdp-request-id").ShouldContain(expectedCorrelationId);
            req.Headers.GetValues("product-name").ShouldContain("TestProduct");
            req.Headers.GetValues("environment-name").ShouldContain("Dev");
            req.RequestUri.ShouldNotBeNull();
            req.RequestUri.AbsoluteUri.ShouldStartWith("https://example.com/");
            req.RequestUri.AbsoluteUri.ShouldContain("evaluate/TestGroup");

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    """
                    {
                      "group_name": "TestGroup",
                      "group_enabled": true,
                      "features": [],
                      "success": true
                    }
                    """,
                    System.Text.Encoding.UTF8,
                    "application/json"),
            };
        });

        var httpClient = new HttpClient(handler);
        var featureFlagHttpClient = new FeatureFlagHttpClient(httpClient, this.options, this.logger);

        // Act
        var result =
            await featureFlagHttpClient.GetFeatureFlagGroupStatusAsync(
                "TestGroup",
                TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.GroupName.ShouldBe("TestGroup");
        result.GroupEnabled.ShouldBeTrue();

        logger.Received(1).Log(
            LogLevel.Debug,
            Arg.Any<EventId>(),
            Arg.Is<object>(v => v.ToString() == "Sending request to Feature Flag API. GroupName: TestGroup"),
            null,
            Arg.Any<Func<object, Exception?, string>>());
        AssertNothingLoggedAtOrAbove(LogLevel.Information);
    }

    [Fact]
    public async Task GetFeatureFlagStatusAsync_ShouldHandleSnakeCaseResponse_WhenApiReturnsSnakeCase()
    {
        // Arrange
        var jsonResponse = """
                           {
                             "flag_name": "TestFlag",
                             "flag_enabled": true,
                             "success": true
                           }
                           """;

        var handler = new TestHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(
                jsonResponse,
                System.Text.Encoding.UTF8,
                "application/json"),
        });

        var httpClient = new HttpClient(handler);
        var featureFlagHttpClient = new FeatureFlagHttpClient(httpClient, this.options, this.logger);

        // Act
        var result =
            await featureFlagHttpClient.GetFeatureFlagStatusAsync("G", "F", TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.FlagName.ShouldBe("TestFlag");
        result.FlagEnabled.ShouldBeTrue();
        result.Success.ShouldBeTrue();
    }

    [Fact]
    public async Task GetFeatureFlagGroupStatusAsync_ShouldHandleSnakeCaseResponse_WhenApiReturnsSnakeCase()
    {
        // Arrange
        var jsonResponse = """
                           {
                             "group_name": "TestGroup",
                             "group_enabled": true,
                             "features": [
                               {
                                 "flag_name": "Flag1",
                                 "flag_enabled": true
                               }
                             ],
                             "success": true
                           }
                           """;

        var handler = new TestHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(
                jsonResponse,
                System.Text.Encoding.UTF8,
                "application/json"),
        });

        var httpClient = new HttpClient(handler);
        var featureFlagHttpClient = new FeatureFlagHttpClient(httpClient, this.options, this.logger);

        // Act
        var result =
            await featureFlagHttpClient.GetFeatureFlagGroupStatusAsync("G", TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.GroupName.ShouldBe("TestGroup");
        result.GroupEnabled.ShouldBeTrue();
        result.Features.Count.ShouldBe(1);
        result.Features[0].FlagName.ShouldBe("Flag1");
        result.Features[0].FlagEnabled.ShouldBeTrue();
        result.Success.ShouldBeTrue();
    }

    [Fact]
    public async Task GetFeatureFlagGroupStatusAsync_ShouldHandlePascalCaseResponse_EvenWithSnakeCasePolicy()
    {
        // Arrange
        const string jsonResponse = """
                                    {
                                      "group_name": "TestGroup",
                                      "group_enabled": true,
                                      "features": [
                                        {
                                          "flag_name": "Flag1",
                                          "flag_enabled": true
                                        }
                                      ],
                                      "success": true
                                    }
                                    """;

        var handler = new TestHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(
                jsonResponse,
                System.Text.Encoding.UTF8,
                "application/json"),
        });

        var httpClient = new HttpClient(handler);
        var featureFlagHttpClient = new FeatureFlagHttpClient(httpClient, this.options, this.logger);

        // Act
        var result =
            await featureFlagHttpClient.GetFeatureFlagGroupStatusAsync("G", TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.GroupName.ShouldBe("TestGroup");
        result.Features[0].FlagName.ShouldBe("Flag1");
    }

    [Fact]
    public async Task GetFeatureFlagGroupStatusAsync_ShouldThrow_WhenFlagNameIsMissing()
    {
        // Arrange
        var jsonResponse = """
                           {
                             "groupName": "TestGroup",
                             "groupEnabled": true,
                             "features": [
                               {
                                 "flagEnabled": true
                               }
                             ],
                             "success": true
                           }
                           """;

        var handler = new TestHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(
                jsonResponse,
                System.Text.Encoding.UTF8,
                "application/json"),
        });

        var httpClient = new HttpClient(handler);
        var featureFlagHttpClient = new FeatureFlagHttpClient(httpClient, this.options, this.logger);

        // Act & Assert
        await Should.ThrowAsync<System.Text.Json.JsonException>(async () =>
            await featureFlagHttpClient.GetFeatureFlagGroupStatusAsync("G", TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task GetFeatureFlagStatusAsync_ShouldEncodeUrlParameters()
    {
        // Arrange
        var groupName = "Group Name With Space";
        var flagName = "Flag/Name/With/Slash";
        var expectedUrlPart = "evaluate/Group%20Name%20With%20Space/Flag%2FName%2FWith%2FSlash";

        var handler = new TestHttpMessageHandler(req =>
        {
            req.RequestUri!.AbsoluteUri.ShouldContain(expectedUrlPart);

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    """
                    {
                      "flag_name": "TestFlag",
                      "flag_enabled": true,
                      "success": true
                    }
                    """,
                    System.Text.Encoding.UTF8,
                    "application/json"),
            };
        });

        var httpClient = new HttpClient(handler);
        var featureFlagHttpClient = new FeatureFlagHttpClient(httpClient, this.options, this.logger);

        // Act
        await featureFlagHttpClient.GetFeatureFlagStatusAsync(
            groupName,
            flagName,
            TestContext.Current.CancellationToken);

        // Assert - handled in handler
    }

    [Fact]
    public async Task GetFeatureFlagGroupStatusAsync_ShouldEncodeUrlParameters()
    {
        // Arrange
        var groupName = "Group Name With Space";
        var expectedUrlPart = "evaluate/Group%20Name%20With%20Space";

        var handler = new TestHttpMessageHandler(req =>
        {
            req.RequestUri!.AbsoluteUri.ShouldContain(expectedUrlPart);

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    """
                    {
                      "group_name": "TestGroup",
                      "group_enabled": true,
                      "features": [],
                      "success": true
                    }
                    """,
                    System.Text.Encoding.UTF8,
                    "application/json"),
            };
        });

        var httpClient = new HttpClient(handler);
        var featureFlagHttpClient = new FeatureFlagHttpClient(httpClient, this.options, this.logger);

        // Act
        await featureFlagHttpClient.GetFeatureFlagGroupStatusAsync(groupName, TestContext.Current.CancellationToken);

        // Assert - handled in handler
    }

    [Fact]
    public async Task GetFeatureFlagStatusAsync_ShouldNotEncodeHeaderParameters()
    {
        // Arrange
        var productName = "Product Name With Space";
        var environmentName = "Env/Name/With/Slash";

        options.Value.ProductName = productName;
        options.Value.EnvironmentName = environmentName;
        options.Value.WithCorrelationId(() => correlationId);

        var handler = new TestHttpMessageHandler(req =>
        {
            req.Headers.GetValues("product-name").ShouldContain(productName);
            req.Headers.GetValues("environment-name").ShouldContain(environmentName);

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    """
                    {
                      "flag_name": "TestFlag",
                      "flag_enabled": true,
                      "success": true
                    }
                    """,
                    System.Text.Encoding.UTF8,
                    "application/json"),
            };
        });

        var httpClient = new HttpClient(handler);
        var featureFlagHttpClient = new FeatureFlagHttpClient(httpClient, this.options, this.logger);

        // Act
        await featureFlagHttpClient.GetFeatureFlagStatusAsync("G", "F", TestContext.Current.CancellationToken);

        // Assert - handled in handler
    }

    [Fact]
    public async Task GetFeatureFlagStatusAsync_ShouldLogWarningWithStatusCode_WhenApiReturnsNonSuccess()
    {
        // Arrange
        options.Value.WithCorrelationId(() => correlationId);

        var handler = new TestHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.ServiceUnavailable));
        var featureFlagHttpClient = new FeatureFlagHttpClient(new HttpClient(handler), this.options, this.logger);

        // Act
        await Should.ThrowAsync<HttpRequestException>(() =>
            featureFlagHttpClient.GetFeatureFlagStatusAsync("TestGroup", "TestFlag", TestContext.Current.CancellationToken));

        // Assert
        logger.Received(1).Log(
            LogLevel.Warning,
            Arg.Any<EventId>(),
            Arg.Is<object>(v =>
                v.ToString() == "Feature Flag API returned status code 503. GroupName: TestGroup, FlagName: TestFlag"),
            null,
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task GetFeatureFlagGroupStatusAsync_ShouldLogWarningWithStatusCode_WhenApiReturnsNonSuccess()
    {
        // Arrange
        options.Value.WithCorrelationId(() => correlationId);

        var handler = new TestHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.NotFound));
        var featureFlagHttpClient = new FeatureFlagHttpClient(new HttpClient(handler), this.options, this.logger);

        // Act
        await Should.ThrowAsync<HttpRequestException>(() =>
            featureFlagHttpClient.GetFeatureFlagGroupStatusAsync("TestGroup", TestContext.Current.CancellationToken));

        // Assert
        logger.Received(1).Log(
            LogLevel.Warning,
            Arg.Any<EventId>(),
            Arg.Is<object>(v => v.ToString() == "Feature Flag API returned status code 404. GroupName: TestGroup"),
            null,
            Arg.Any<Func<object, Exception?, string>>());
    }

    private void AssertNothingLoggedAtOrAbove(LogLevel minimumLevel)
    {
        logger.ReceivedCalls()
            .Where(c => c.GetMethodInfo().Name == nameof(ILogger.Log))
            .Select(c => (LogLevel)c.GetArguments()[0]!)
            .ShouldAllBe(level => level < minimumLevel);
    }

    private class TestHttpMessageHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> handler;

        public TestHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> handler)
        {
            this.handler = handler;
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(handler(request));
        }
    }
}
