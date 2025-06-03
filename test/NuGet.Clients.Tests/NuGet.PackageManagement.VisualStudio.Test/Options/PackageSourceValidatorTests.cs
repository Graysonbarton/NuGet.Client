// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#nullable enable

using System;
using System.Collections.Generic;
using FluentAssertions;
using NuGet.Configuration;
using NuGet.PackageManagement.VisualStudio.Options;
using Xunit;

namespace NuGet.PackageManagement.VisualStudio.Test.Options
{
    public class PackageSourceValidatorTests
    {
        [Theory]
        [InlineData("TestSource", "TestSource")]
        [InlineData(" TestSource ", " TestSource ")]
        [InlineData("TestSource ", "TestSource ")]
        public void PrepareForSave_ExactDuplicateSourceNames_ThrowsArgumentException(string name1, string name2)
        {
            // Arrange
            var packageSources = new List<PackageSource>
            {
                new PackageSource(source: "https://testsource1.com", name1),
                new PackageSource(source: "https://testsource2.com", name2)
            };

            // Act
            Action act = () => PackageSourceValidator.PrepareForSave(packageSources);

            // Assert
            ArgumentException exception = Assert.Throws<ArgumentException>(act);
            exception.Message.Should().Be(Strings.Error_PackageSource_UniqueName);
        }

        [Theory]
        [InlineData(" TestSource", "TestSource")]
        [InlineData("TestSource", " TestSource")]
        [InlineData("TestSource ", "TestSource")]
        [InlineData("TestSource", "TestSource ")]
        public void PrepareForSave_DifferentWhitespaceDuplicateSourceNames_Successful(string name1, string name2)
        {
            // Arrange
            var packageSources = new List<PackageSource>
            {
                new PackageSource(source: "https://testsource1.com", name1),
                new PackageSource(source: "https://testsource2.com", name2)
            };

            // Act
            PackageSourceValidator.PrepareForSave(packageSources);

            // Assert
            // No exception should be thrown, indicating success.
        }

        [Theory]
        [InlineData("https://testsource.com", "https://testsource.com")]
        [InlineData("https://testsource.com", "https://testsource.com/")]
        public void PrepareForSave_DuplicateRemoteSources_ThrowsArgumentException(string source1, string source2)
        {
            // Arrange
            var packageSources = new List<PackageSource>
            {
                new PackageSource(source1, name: "TestSource1"),
                new PackageSource(source2, name: "TestSource2")
            };

            // Act
            Action act = () => PackageSourceValidator.PrepareForSave(packageSources);

            // Assert
            ArgumentException exception = Assert.Throws<ArgumentException>(act);
            exception.Message.Should().Be(Strings.Error_PackageSource_UniqueSource);
        }

        [Theory]
        [InlineData(@"\\server\share", @"\\server\share\")]
        public void PrepareForSave_DuplicatePathSources_ThrowsArgumentException(string firstSource, string secondSource)
        {
            // Arrange
            var packageSources = new List<PackageSource>
            {
                new PackageSource(source: firstSource, name: "TestSource1"),
                new PackageSource(source: secondSource, name: "TestSource2")
            };

            // Act
            Action act = () => PackageSourceValidator.PrepareForSave(packageSources);

            // Assert
            ArgumentException exception = Assert.Throws<ArgumentException>(act);
            exception.Message.Should().Be(Strings.Error_PackageSource_UniqueSource);
        }

        [Theory]
        [InlineData(@"C")]
        [InlineData(@"C:")]
        [InlineData(@"\\server\invalid\*\")]
        [InlineData(@"..\packages")]
        public void ValidatePathOrThrow_InvalidPath_ThrowsArgumentOutOfRangeException(string invalidSource)
        {
            // Arrange
            var packageSource = new PackageSource(source: invalidSource, name: "TestInvalidSource");

            // Act
            Action act = () => PackageSourceValidator.ValidatePathOrThrow(packageSource);

            // Assert
            ArgumentOutOfRangeException exception = Assert.Throws<ArgumentOutOfRangeException>(act);
            exception.ParamName.Should().Be(nameof(PackageSource.Source));
            exception.Message.Should().StartWith(Strings.Error_PackageSource_InvalidSource);
        }

        //[InlineData(@"https://test")] // Valid
    }
}
