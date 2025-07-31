// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using NuGet.Commands.Experimental.Services;
using NuGet.Configuration;
using NuGet.Protocol.Core.Types;
using Xunit;
using static NuGet.Test.Utility.V3PackageSearchMetadataFixture;

namespace NuGet.Commands.Test.Services
{
    public class PackageMetadataSourceResultTests
    {
        [Fact]
        public void Constructor_WithValidArguments_SetsProperties()
        {
            // Arrange
            IPackageSearchMetadata metadata = new MockPackageSearchMetadata();
            PackageSource packageSource = new("https://source", "source");

            // Act
            var result = new PackageMetadataSourceResult(metadata, packageSource);

            // Assert
            Assert.Equal(metadata, result.Metadata);
            Assert.Equal(packageSource, result.Source);
        }

        [Fact]
        public void Constructor_WithNullMetadata_ThrowsArgumentNullException()
        {
            // Arrange
            IPackageSearchMetadata metadata = null;
            PackageSource packageSource = new("https://source", "source");

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new PackageMetadataSourceResult(metadata, packageSource));
        }

        [Fact]
        public void Constructor_WithNullSource_ThrowsArgumentNullException()
        {
            // Arrange
            IPackageSearchMetadata metadata = new MockPackageSearchMetadata();
            PackageSource source = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new PackageMetadataSourceResult(metadata, source));
        }
    }
}
