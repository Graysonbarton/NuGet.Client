// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#nullable enable

using NuGet.PackageManagement.UI.Models.Package;
using NuGet.VisualStudio.Internal.Contracts;
using Xunit;

namespace NuGet.PackageManagement.UI.Test.Models.Package
{
    public class DeprecationCapabilityBaseTests
    {
        [Fact]
        public void IsDeprecated_WithDeprecationMetadata_IsTrue()
        {
            // Arrange
            var deprecationMetadata = new PackageDeprecationMetadataContextInfo("Test message", ["Legacy"], null);

            var capability = new TestDeprecationCapability(deprecationMetadata);

            // Act
            var result = capability.IsDeprecated;

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsDeprecated_WithoutDeprecationMetadata_IsFalse()
        {
            // Arrange
            var capability = new TestDeprecationCapability(null);

            // Act
            var result = capability.IsDeprecated;

            // Assert
            Assert.False(result);
        }

        [Theory]
        [InlineData(new[] { "CriticalBugs" }, PackageDeprecationReason.CriticalBugs)]
        [InlineData(new[] { "criticalbugs" }, PackageDeprecationReason.CriticalBugs)]
        [InlineData(new[] { "Legacy" }, PackageDeprecationReason.Legacy)]
        [InlineData(new[] { "Legacy", "CriticalBugs" }, PackageDeprecationReason.LegacyAndCriticalBugs)]
        [InlineData(new[] { "Other" }, PackageDeprecationReason.Unknown)]
        public void PackageDeprecationReasons_MultipleDeprecationReasons_ReturnsExpected(string[] reasons, PackageDeprecationReason expectedMessage)
        {
            // Arrange
            var deprecationMetadata = new PackageDeprecationMetadataContextInfo("Test message", reasons, null);

            var capability = new TestDeprecationCapability(deprecationMetadata);

            // Act
            var result = capability.PackageDeprecationReasons;

            // Assert
            Assert.Equal(expectedMessage, result);
        }
    }
}
