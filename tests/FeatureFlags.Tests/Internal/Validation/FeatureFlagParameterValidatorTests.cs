// <copyright file="FeatureFlagParameterValidatorTests.cs" company="Defra">
// Copyright (c) Defra. All rights reserved.
// </copyright>

namespace Defra.Livestock.Sdk.Api.FeatureFlags.Tests.Internal.Validation;

using Defra.Livestock.Sdk.Api.FeatureFlags.Exceptions;
using Defra.Livestock.Sdk.Api.FeatureFlags.Internal.Validation;
using Shouldly;
using Xunit;

public class FeatureFlagParameterValidatorTests
{
    [Theory]
#pragma warning disable xUnit1012
    [InlineData(null)]
#pragma warning restore xUnit1012
    [InlineData("")]
    [InlineData("   ")]
    public void ValidateGroupName_ShouldThrowFeatureFlagParameterException_WhenGroupNameIsMissing(string groupName)
    {
        // Act
        var exception = Record.Exception(() => FeatureFlagParameterValidator.ValidateGroupName(groupName));

        // Assert
        exception.ShouldBeOfType<FeatureFlagParameterException>();
        exception.Message.ShouldBe("FeatureFlag GroupName is required");
    }

    [Fact]
    public void ValidateGroupName_ShouldThrowFeatureFlagParameterException_WhenGroupNameIsTooLong()
    {
        // Arrange
        var groupName = new string('a', 101);

        // Act
        var exception = Record.Exception(() => FeatureFlagParameterValidator.ValidateGroupName(groupName));

        // Assert
        exception.ShouldBeOfType<FeatureFlagParameterException>();
        exception.Message.ShouldBe("FeatureFlag GroupName cannot exceed 100 characters");
    }

    [Fact]
    public void ValidateGroupName_ShouldNotThrow_WhenGroupNameIsValid()
    {
        // Arrange
        const string groupName = "ValidGroup";

        // Act
        var exception = Record.Exception(() => FeatureFlagParameterValidator.ValidateGroupName(groupName));

        // Assert
        exception.ShouldBeNull();
    }

    [Theory]
#pragma warning disable xUnit1012
    [InlineData(null)]
#pragma warning restore xUnit1012
    [InlineData("")]
    [InlineData("   ")]
    public void ValidateFlagName_ShouldThrowFeatureFlagParameterException_WhenFlagNameIsMissing(string flagName)
    {
        // Act
        var exception = Record.Exception(() => FeatureFlagParameterValidator.ValidateFlagName(flagName));

        // Assert
        exception.ShouldBeOfType<FeatureFlagParameterException>();
        exception.Message.ShouldBe("FeatureFlag FlagName is required");
    }

    [Fact]
    public void ValidateFlagName_ShouldNotThrow_WhenFlagNameIsValid()
    {
        // Arrange
        const string flagName = "ValidFlag";

        // Act
        var exception = Record.Exception(() => FeatureFlagParameterValidator.ValidateFlagName(flagName));

        // Assert
        exception.ShouldBeNull();
    }
}
