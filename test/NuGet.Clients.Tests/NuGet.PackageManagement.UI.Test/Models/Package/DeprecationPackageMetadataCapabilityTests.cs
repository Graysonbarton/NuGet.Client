// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#nullable enable

using System;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using NuGet.PackageManagement.UI.Models.Package;
using NuGet.Versioning;
using NuGet.VisualStudio.Internal.Contracts;
using Xunit;

namespace NuGet.PackageManagement.UI.Test.Models.Package
{
    public class DeprecationPackageMetadataCapabilityTests
    {
        private readonly Mock<IPackageMetadataRetrievalAdapter> _packageMetadataRetrievalAdapterMock = new Mock<IPackageMetadataRetrievalAdapter>();


        public DeprecationPackageMetadataCapabilityTests()
        {
            _packageMetadataRetrievalAdapterMock = new Mock<IPackageMetadataRetrievalAdapter>();
        }

        [Fact]
        public void Constructor_WithNullPackageMetadataRetrievalAdapter_ThrowsArgumentNullException()
        {
            // Arrange
            IPackageMetadataRetrievalAdapter? packageMetadataRetrievalAdapter = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new DeprecationPackageMetadataCapability(packageMetadataRetrievalAdapter!));
        }

        [Fact]
        public async Task PopulateDataAsync_WithValidData_PopulatesDeprecationMetadata()
        {
            // Arrange
            var alternatePackageMetadata = new AlternatePackageMetadataContextInfo("Test.Package", It.IsAny<VersionRange>());
            var deprecationMetadata = new PackageDeprecationMetadataContextInfo("Test message", ["Legacy"], alternatePackageMetadata);
            _packageMetadataRetrievalAdapterMock.Setup(pm => pm.GetPackageDeprecationInfoAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(deprecationMetadata);

            var capability = new DeprecationPackageMetadataCapability(_packageMetadataRetrievalAdapterMock.Object);

            // Act
            await capability.PopulateDataAsync(CancellationToken.None);

            // Assert
            Assert.Equal(alternatePackageMetadata, capability.AlternatePackage);
            Assert.Equal(deprecationMetadata, capability.DeprecationMetadata);
        }
    }
}
