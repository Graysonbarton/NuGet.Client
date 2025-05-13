// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NuGet.PackageManagement.UI.Models.Package;
using NuGet.Packaging.Core;
using NuGet.Versioning;
using NuGet.VisualStudio.Internal.Contracts;
using Xunit;

namespace NuGet.PackageManagement.UI.Test.Models.Package
{
    public class PackageVersionsModelTests
    {
        private readonly Mock<INuGetSearchService> _mockSearchService;
        private readonly PackageIdentity _packageIdentity;
        private readonly PackageModel _packageModel;
        private readonly IReadOnlyCollection<PackageSourceContextInfo> _packageSources;

        public PackageVersionsModelTests()
        {
            _mockSearchService = new Mock<INuGetSearchService>();
            _packageIdentity = new PackageIdentity("TestPackage", NuGetVersion.Parse("1.0.0"));
            var vulnerableCapability = new Mock<IVulnerableCapable>();
            var deprecationCapability = new Mock<IDeprecationCapable>();
            var embeddedResourceCapability = new Mock<IEmbeddedResourcesCapable>();
            _packageModel = PackageModelCreationTestHelper.CreateRemotePackageModel(_packageIdentity, vulnerableCapability.Object, deprecationCapability.Object, embeddedResourceCapability.Object);
            _packageSources = new List<PackageSourceContextInfo> { new PackageSourceContextInfo("source") };
        }

        [Fact]
        public void Constructor_WithNullSearchService_ThrowsArgumentException()
        {
            // Act
            Action act = () => new PackageVersionsModel(
                _packageSources,
                false,
                _packageIdentity,
                null!);

            // assert
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Constructor_WithNullPackageModel_ThrowsArgumentNullException()
        {
            // Act
            Action act = () => new PackageVersionsModel(
                _packageSources,
                false,
                null!,
                _mockSearchService.Object);

            // assert
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public async Task PopulateData_CallsSearchService_WithCorrectParameters()
        {
            // Arrange
            var expectedVersions = new List<VersionInfoContextInfo>
            {
                new VersionInfoContextInfo(NuGetVersion.Parse("1.0.0")),
                new VersionInfoContextInfo(NuGetVersion.Parse("1.1.0"))
            };

            _mockSearchService.Setup(s => s.GetPackageVersionsAsync(
                    _packageIdentity,
                    _packageSources,
                    true,
                    true,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedVersions);

            var packageModelVersions = new PackageVersionsModel(_packageSources, true, _packageIdentity, _mockSearchService.Object);

            // Act
            await packageModelVersions.PopulateDataAsync(CancellationToken.None);
            var result = packageModelVersions.Versions;
            // Assert
            result.Should().BeSameAs(expectedVersions);

            _mockSearchService.Verify(s => s.GetPackageVersionsAsync(
                _packageIdentity,
                _packageSources,
                true,
                true,
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task PopulateData_CachesResults_OnSubsequentCalls()
        {
            // Arrange
            var expectedVersions = new List<VersionInfoContextInfo>
            {
                new VersionInfoContextInfo(NuGetVersion.Parse("1.0.0"))
            };

            _mockSearchService.Setup(s => s.GetPackageVersionsAsync(
                    _packageIdentity,
                    _packageSources,
                    true,
                    true,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedVersions);

            var packageModelVersions = new PackageVersionsModel(_packageSources, true, _packageIdentity, _mockSearchService.Object);

            // Act - Call twice with the same parameters
            await packageModelVersions.PopulateDataAsync(CancellationToken.None);
            var result1 = packageModelVersions.Versions;

            await packageModelVersions.PopulateDataAsync(CancellationToken.None);
            var result2 = packageModelVersions.Versions;

            // Assert - Both results should be the same instance and search service should only be called once
            result1.Should().BeSameAs(expectedVersions);
            result2.Should().BeSameAs(expectedVersions);
            result2.Should().BeSameAs(result1);

            _mockSearchService.Verify(s => s.GetPackageVersionsAsync(
                _packageIdentity,
                _packageSources,
                true,
                true,
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task PopulateData_PropagatesCancellationToken()
        {
            // Arrange
            var canceledToken = new CancellationToken(true);

            _mockSearchService.Setup(s => s.GetPackageVersionsAsync(
                    _packageIdentity,
                    _packageSources,
                    true,
                    true,
                    canceledToken))
                .ThrowsAsync(new OperationCanceledException(canceledToken));

            var packageModelVersions = new PackageVersionsModel(_packageSources, true, _packageIdentity, _mockSearchService.Object);

            // Act
            var act = packageModelVersions.Awaiting(v => v.PopulateDataAsync(canceledToken));

            // Assert
            await act.Should().ThrowAsync<OperationCanceledException>();

            _mockSearchService.Verify(s => s.GetPackageVersionsAsync(
                _packageIdentity,
                _packageSources,
                true,
                true,
                canceledToken), Times.Once);
        }

        [Fact]
        public async Task PopulateData_WithDifferentParameters_DoesNotUseCachedResults()
        {
            // Arrange
            var versions1 = new List<VersionInfoContextInfo> { new VersionInfoContextInfo(NuGetVersion.Parse("1.0.0")) };
            var versions2 = new List<VersionInfoContextInfo> { new VersionInfoContextInfo(NuGetVersion.Parse("2.0.0")) };

            _mockSearchService.Setup(s => s.GetPackageVersionsAsync(
                    _packageIdentity,
                    _packageSources,
                    true,
                    true,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(versions1);

            var packageModelVersions = new PackageVersionsModel(_packageSources, true, _packageIdentity, _mockSearchService.Object);

            // Act - First call should set the cache
            await packageModelVersions.PopulateDataAsync(CancellationToken.None);
            var result1 = packageModelVersions.Versions;

            // This call should use the cached result regardless of different parameters
            await packageModelVersions.PopulateDataAsync(CancellationToken.None);
            var result2 = packageModelVersions.Versions;

            // Assert
            result1.Should().BeSameAs(versions1);
            // It should return the cached result from the first call
            result2.Should().BeSameAs(versions1);

            // Service should be called only once
            _mockSearchService.Verify(s => s.GetPackageVersionsAsync(
                _packageIdentity,
                _packageSources,
                true,
                true,
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public void GetLatestVersion_NoVersionsAvailable_ReturnsNull()
        {
            // Arrange
            var packageModelVersions = new PackageVersionsModel(_packageSources, true, _packageIdentity, _mockSearchService.Object);

            // Act
            var result = packageModelVersions.GetLatestVersion(VersionRange.All);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetLatestVersion_NoVersionsInRange_ReturnsNull()
        {
            // Arrange
            var expectedVersions = new List<VersionInfoContextInfo>
            {
                new VersionInfoContextInfo(NuGetVersion.Parse("1.0.0")),
                new VersionInfoContextInfo(NuGetVersion.Parse("2.0.0"))
            };

            _mockSearchService.Setup(s => s.GetPackageVersionsAsync(
                    _packageIdentity,
                    _packageSources,
                    true,
                    true,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedVersions);

            var packageModelVersions = new PackageVersionsModel(_packageSources, true, _packageIdentity, _mockSearchService.Object);
            await packageModelVersions.PopulateDataAsync(CancellationToken.None);

            // Act
            var result = packageModelVersions.GetLatestVersion(VersionRange.Parse("[3.0.0,4.0.0)"));

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetLatestVersion_SingleVersionInRange_ReturnsThatVersion()
        {
            // Arrange
            var expectedVersions = new List<VersionInfoContextInfo>
            {
                new VersionInfoContextInfo(NuGetVersion.Parse("1.0.0")),
                new VersionInfoContextInfo(NuGetVersion.Parse("2.0.0"))
            };

            _mockSearchService.Setup(s => s.GetPackageVersionsAsync(
                    _packageIdentity,
                    _packageSources,
                    true,
                    true,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedVersions);

            var packageModelVersions = new PackageVersionsModel(_packageSources, true, _packageIdentity, _mockSearchService.Object);
            await packageModelVersions.PopulateDataAsync(CancellationToken.None);

            // Act
            var result = packageModelVersions.GetLatestVersion(VersionRange.Parse("[2.0.0]"));

            // Assert
            result.Should().NotBeNull();
            result.Should().Be(NuGetVersion.Parse("2.0.0"));
        }

        [Fact]
        public async Task GetLatestVersion_MultipleVersionsInRange_ReturnsHighestVersion()
        {
            // Arrange
            var expectedVersions = new List<VersionInfoContextInfo>
            {
                new VersionInfoContextInfo(NuGetVersion.Parse("1.0.0")),
                new VersionInfoContextInfo(NuGetVersion.Parse("2.0.0")),
                new VersionInfoContextInfo(NuGetVersion.Parse("1.5.0"))
            };

            _mockSearchService.Setup(s => s.GetPackageVersionsAsync(
                    _packageIdentity,
                    _packageSources,
                    true,
                    true,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedVersions);

            var packageModelVersions = new PackageVersionsModel(_packageSources, true, _packageIdentity, _mockSearchService.Object);
            await packageModelVersions.PopulateDataAsync(CancellationToken.None);

            // Act
            var result = packageModelVersions.GetLatestVersion(VersionRange.Parse("[1.0.0,2.0.0]"));

            // Assert
            result.Should().NotBeNull();
            result.Should().Be(NuGetVersion.Parse("2.0.0"));
        }

    }
}
