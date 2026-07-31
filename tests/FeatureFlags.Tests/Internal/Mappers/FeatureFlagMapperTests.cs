// <copyright file="FeatureFlagMapperTests.cs" company="Defra">
// Copyright (c) Defra. All rights reserved.
// </copyright>

namespace Defra.Livestock.Sdk.Api.FeatureFlags.Tests.Internal.Mappers;

using Defra.Livestock.Sdk.Api.FeatureFlags.Internal.Client.Contract.Responses;
using Defra.Livestock.Sdk.Api.FeatureFlags.Internal.Client.Contract.Responses.Common;
using Defra.Livestock.Sdk.Api.FeatureFlags.Internal.Mappers;
using Shouldly;
using Xunit;

public class FeatureFlagMapperTests
{
    [Fact]
    public void MapFromGetFeatureFlagGroupStatusResponse_ShouldMapCorrectly()
    {
        // Arrange
        var response = new GetFeatureFlagGroupStatusResponse
        {
            GroupName = "TestGroup",
            GroupEnabled = true,
            Success = true,
            Features =
            [
                new FeatureFlagStatusResponse { FlagName = "Flag1", FlagEnabled = true },
                new FeatureFlagStatusResponse { FlagName = "Flag2", FlagEnabled = false }
            ],
        };

        // Act
        var result = FeatureFlagMapper.MapFromGetFeatureFlagGroupStatusResponse(response);

        // Assert
        result.ShouldNotBeNull();
        result.GroupName.ShouldBe(response.GroupName);
        result.GroupEnabled.ShouldBe(response.GroupEnabled);
        result.Success.ShouldBe(response.Success);
        result.Features.Count.ShouldBe(2);

        result.Features[0].FlagName.ShouldBe("Flag1");
        result.Features[0].FlagEnabled.ShouldBeTrue();
        result.Features[0].Success.ShouldBeTrue();

        result.Features[1].FlagName.ShouldBe("Flag2");
        result.Features[1].FlagEnabled.ShouldBeFalse();
        result.Features[1].Success.ShouldBeTrue();
    }

    [Fact]
    public void MapFromGetFeatureFlagStatusResponse_ShouldMapCorrectly()
    {
        // Arrange
        var response = new GetFeatureFlagStatusResponse { FlagName = "TestFlag", FlagEnabled = true, Success = true, };

        // Act
        var result = FeatureFlagMapper.MapFromGetFeatureFlagStatusResponse(response);

        // Assert
        result.ShouldNotBeNull();
        result.FlagName.ShouldBe(response.FlagName);
        result.FlagEnabled.ShouldBe(response.FlagEnabled);
        result.Success.ShouldBe(response.Success);
    }
}
