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
        public void ValidateForSave_ExactDuplicateSourceNames_ThrowsArgumentException(string name1, string name2)
        {
            // Arrange
            var packageSources = new List<PackageSource>
            {
                new PackageSource(source: "https://testsource1.com", name1),
                new PackageSource(source: "https://testsource2.com", name2)
            };

            // Act
            Action act = () => PackageSourceValidator.ValidateUniquenessOrThrow(packageSources);

            // Assert
            ArgumentException exception = Assert.Throws<ArgumentException>(act);
            exception.Message.Should().Be(Strings.Error_PackageSource_UniqueName);
        }

        [Theory]
        [InlineData(" TestSource", "TestSource")]
        [InlineData("TestSource", " TestSource")]
        [InlineData("TestSource ", "TestSource")]
        [InlineData("TestSource", "TestSource ")]
        public void ValidateForSave_DifferentWhitespaceDuplicateSourceNames_Successful(string name1, string name2)
        {
            // Arrange
            var packageSources = new List<PackageSource>
            {
                new PackageSource(source: "https://testsource1.com", name1),
                new PackageSource(source: "https://testsource2.com", name2)
            };

            // Act
            PackageSourceValidator.ValidateUniquenessOrThrow(packageSources);

            // Assert
            // No exception should be thrown, indicating success.
        }

        [Theory]
        [InlineData("https://testsource.com", "https://testsource.com")]
        [InlineData("https://testsource.com", "https://testsource.com/")]
        public void ValidateForSave_DuplicateRemoteSources_ThrowsArgumentException(string source1, string source2)
        {
            // Arrange
            var packageSources = new List<PackageSource>
            {
                new PackageSource(source1, name: "TestSource1"),
                new PackageSource(source2, name: "TestSource2")
            };

            // Act
            Action act = () => PackageSourceValidator.ValidateUniquenessOrThrow(packageSources);

            // Assert
            ArgumentException exception = Assert.Throws<ArgumentException>(act);
            exception.Message.Should().Be(Strings.Error_PackageSource_UniqueSource);
        }

        [InlineData(@"custom://testsource.com/")]
        [InlineData(@"https://testsource.com/")]
        [InlineData(@"https://api.nuget.org/v3/index.json")]
        public void ValidateForSave_DifferentWhitespaceDuplicateRemoteSources_ThrowsArgumentException(string source1, string source2)
        {
            // Arrange
            var packageSources = new List<PackageSource>
            {
                new PackageSource(source1, name: "TestSource1"),
                new PackageSource(source2, name: "TestSource2")
            };

            // Act
            Action act = () => PackageSourceValidator.ValidateUniquenessOrThrow(packageSources);

            // Assert
            ArgumentException exception = Assert.Throws<ArgumentException>(act);
            exception.Message.Should().Be(Strings.Error_PackageSource_UniqueSource);
        }

        [Theory]
        [InlineData(@"\\server\share", @"\\server\share\")]
        [InlineData(@"C:\path", @"C:\path\")]
        [InlineData(@"C:\path\to", @"C:\path\to\")]
        public void ValidateForSave_DuplicatePathSources_ThrowsArgumentException(string firstSource, string secondSource)
        {
            // Arrange
            var packageSources = new List<PackageSource>
            {
                new PackageSource(source: firstSource, name: "TestSource1"),
                new PackageSource(source: secondSource, name: "TestSource2")
            };

            // Act
            Action act = () => PackageSourceValidator.ValidateUniquenessOrThrow(packageSources);

            // Assert
            ArgumentException exception = Assert.Throws<ArgumentException>(act);
            exception.Message.Should().Be(Strings.Error_PackageSource_UniqueSource);
        }

        //[Theory]
        //[InlineData(@"http:/")]
        //[InlineData(@"http://")]
        //[InlineData(@"https://")]
        //[InlineData(@"https:// ")]
        //[InlineData(@" https://")]
        //[InlineData(@"ftp://")]
        //[InlineData(@"ftp://testsource.com")]
        //[InlineData(@"custom://testsource.com/")]
        //public void ValidatePathOrThrow_InvalidRemoteSource_Successful(string validSource)
        //{
        //    // Arrange
        //    var packageSources = new List<PackageSource>
        //    {
        //        new PackageSource(source: validSource, name: "TestSource1"),
        //    };

        //    // Act
        // TODO: Manually test the regex out of the registration.json file.

        //    // Assert
        //    ArgumentException exception = Assert.Throws<ArgumentException>(act);
        //    exception.Message.Should().Be(Strings.Error_PackageSource_UniqueSource);
        //}


        [Theory]
        [InlineData(@"custom://testsource.com/")]
        [InlineData(@"https://testsource.com/")]
        [InlineData(@"https://api.nuget.org/v3/index.json")]
        public void ValidateForSave_ValidRemoteSources_Successful(string validSource)
        {
            // Arrange
            var packageSources = new List<PackageSource>
            {
                new PackageSource(validSource, name: "TestSource1"),
            };

            // Act
            //Action act = () => PackageSourceValidator.validate(packageSources);
            //

            // Assert
            ArgumentException exception = Assert.Throws<ArgumentException>(act);
            exception.Message.Should().Be(Strings.Error_PackageSource_UniqueSource);
        }

        [Theory]
        [InlineData(@"https://1")]
        [InlineData(@"https://testsource.com")]
        [InlineData(@"https://testsource.com/")]
        [InlineData(@"ftp://1")]
        [InlineData(@"ftp://testsource.com")]
        [InlineData(@"custom://testsource.com/")]
        [InlineData(@"https://api.nuget.org/v3/index.json")]
        public void ValidateForSave_ValidRemoteSource_Successful(string validSource)
        {
            // Arrange
            var packageSources = new List<PackageSource>
            {
                new PackageSource(source: validSource, name: "TestSource1"),
            };

            // Act
            // TODO: Manually test the regex out of the registration.json file.

            // Assert
            // No exception should be thrown, indicating success.
        }

        [Theory]
        [InlineData(@"C")]
        [InlineData(@"http")] // Missing :// causes this to be treated as a file path.
        [InlineData(@"http:")]
        [InlineData(@"ftp")] // Missing :// causes this to be treated as a file path.
        [InlineData(@"C:")]
        [InlineData(@"C:\invalid\*\'\chars")]
        [InlineData(@"\\server\invalid\*\")]
        [InlineData(@"..\packages")]
        public void ValidatePathOrThrow_InvalidUncPath_ThrowsArgumentOutOfRangeException(string invalidSource)
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

        [Theory]
        //C:\, C:\path, C:\path\to\
        [InlineData(@"C:\")] // Valid UNC
        [InlineData(@"C:\path")]
        [InlineData(@"C:\path\")]
        [InlineData(@"C:\path\to")]
        [InlineData(@"C:\path\to\")]
        public void ValidatePathOrThrow_ValidUncPath_Successful(string validSource)
        {
            // Arrange
            var packageSource = new PackageSource(source: validSource, name: "TestValidSource");

            // Act
            PackageSourceValidator.ValidatePathOrThrow(packageSource);

            // Assert
            // No exception should be thrown, indicating success.
        }
    }
}
