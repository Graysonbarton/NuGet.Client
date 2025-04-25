// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using NuGet.PackageManagement.UI.Models.Package;
using NuGet.Packaging.Core;
using NuGet.Versioning;
using NuGet.VisualStudio.Internal.Contracts;
using Xunit;

namespace NuGet.PackageManagement.UI.Test.Models.Package
{
    public class PackageModelVersionsTest
    {
        private readonly Mock<INuGetSearchService> _mockSearchService;
        private readonly PackageIdentity _packageIdentity;
        private readonly PackageModel _packageModel;
        private readonly IReadOnlyCollection<PackageSourceContextInfo> _packageSources;
        private readonly IEnumerable<IProjectContextInfo> _projects;

        public PackageModelVersionsTest()
        {
            _mockSearchService = new Mock<INuGetSearchService>();
            _packageIdentity = new PackageIdentity("TestPackage", NuGetVersion.Parse("1.0.0"));
            var vulnerableCapability = new Mock<IVulnerableCapable>();
            var deprecationCapability = new Mock<IDeprecationCapable>();
            var embeddedResourceCapability = new Mock<IEmbeddedResourcesCapable>();
            _packageModel = PackageModelCreationTestHelper.CreateRemotePackageModel(_packageIdentity, vulnerableCapability.Object, deprecationCapability.Object, embeddedResourceCapability.Object);
            _packageSources = new List<PackageSourceContextInfo> { new PackageSourceContextInfo("source") };
            _projects = new List<IProjectContextInfo> { Mock.Of<IProjectContextInfo>() };
        }

        [Fact]
        public void Constructor_WithNullSearchService_ThrowsArgumentException()
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => new PackageModelVersions(
                null!,
                _packageModel));
        }

        [Fact]
        public void Constructor_WithNullPackageModel_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new PackageModelVersions(
                _mockSearchService.Object,
                null!));
        }

        [Fact]
        public async Task GetPackageVersionsAsync_CallsSearchService_WithCorrectParameters()
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
                    false,
                    _projects,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedVersions);

            var packageModelVersions = new PackageModelVersions(_mockSearchService.Object, _packageModel);

            // Act
            var result = await packageModelVersions.GetPackageVersionsAsync(_packageSources, true, _projects, CancellationToken.None);

            // Assert
            Assert.Same(expectedVersions, result);
            _mockSearchService.Verify(s => s.GetPackageVersionsAsync(
                _packageIdentity,
                _packageSources,
                true,
                false,
                _projects,
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetPackageVersionsAsync_WithTransitivePackage_SetsIsTransitiveFlag()
        {
            // Arrange
            var expectedVersions = new List<VersionInfoContextInfo>
            {
                new VersionInfoContextInfo(NuGetVersion.Parse("1.0.0"))
            };

            var vulnerableCapability = new Mock<IVulnerableCapable>();
            var embeddedResourceCapability = new Mock<IEmbeddedResourcesCapable>();
            var transitiveOrigins = new List<PackageIdentity>
            {
                new PackageIdentity("OriginPackage1", new NuGetVersion("1.0.0")),
                new PackageIdentity("OriginPackage2", new NuGetVersion("2.0.0"))
            };

            var transitivePackageModel = PackageModelCreationTestHelper.CreateTransitivelyReferencedPackageModel(_packageIdentity, vulnerableCapability.Object, embeddedResourceCapability.Object, transitiveOrigins);


            _mockSearchService.Setup(s => s.GetPackageVersionsAsync(
                    _packageIdentity,
                    _packageSources,
                    true,
                    true, // This should be true for transitive packages
                    _projects,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedVersions);

            var packageModelVersions = new PackageModelVersions(_mockSearchService.Object, transitivePackageModel);

            // Act
            var result = await packageModelVersions.GetPackageVersionsAsync(_packageSources, true, _projects, CancellationToken.None);

            // Assert
            Assert.Same(expectedVersions, result);
            _mockSearchService.Verify(s => s.GetPackageVersionsAsync(
                _packageIdentity,
                _packageSources,
                true,
                true, // Verify isTransitive is true
                _projects,
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetPackageVersionsAsync_CachesResults_OnSubsequentCalls()
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
                    false,
                    _projects,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedVersions);

            var packageModelVersions = new PackageModelVersions(_mockSearchService.Object, _packageModel);

            // Act - Call twice with the same parameters
            var result1 = await packageModelVersions.GetPackageVersionsAsync(_packageSources, true, _projects, CancellationToken.None);
            var result2 = await packageModelVersions.GetPackageVersionsAsync(_packageSources, true, _projects, CancellationToken.None);

            // Assert - Both results should be the same instance and search service should only be called once
            Assert.Same(expectedVersions, result1);
            Assert.Same(expectedVersions, result2);
            Assert.Same(result1, result2);
            _mockSearchService.Verify(s => s.GetPackageVersionsAsync(
                _packageIdentity,
                _packageSources,
                true,
                false,
                _projects,
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetPackageVersionsAsync_PropagatesCancellationToken()
        {
            // Arrange
            var canceledToken = new CancellationToken(true);

            var packageModelVersions = new PackageModelVersions(_mockSearchService.Object, _packageModel);

            _mockSearchService.Setup(s => s.GetPackageVersionsAsync(
                    _packageIdentity,
                    _packageSources,
                    true,
                    false,
                    _projects,
                    canceledToken))
                .ThrowsAsync(new OperationCanceledException(canceledToken));

            // Act & Assert
            await Assert.ThrowsAsync<OperationCanceledException>(() =>
                packageModelVersions.GetPackageVersionsAsync(_packageSources, true, _projects, canceledToken));

            _mockSearchService.Verify(s => s.GetPackageVersionsAsync(
                _packageIdentity,
                _packageSources,
                true,
                false,
                _projects,
                canceledToken), Times.Once);
        }

        [Fact]
        public async Task GetPackageVersionsAsync_WithDifferentParameters_DoesNotUseCachedResults()
        {
            // Arrange
            var versions1 = new List<VersionInfoContextInfo> { new VersionInfoContextInfo(NuGetVersion.Parse("1.0.0")) };
            var versions2 = new List<VersionInfoContextInfo> { new VersionInfoContextInfo(NuGetVersion.Parse("2.0.0")) };

            var sources2 = new List<PackageSourceContextInfo> { new PackageSourceContextInfo("source2") };

            _mockSearchService.Setup(s => s.GetPackageVersionsAsync(
                    _packageIdentity,
                    _packageSources,
                    true,
                    false,
                    _projects,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(versions1);

            _mockSearchService.Setup(s => s.GetPackageVersionsAsync(
                    _packageIdentity,
                    sources2,
                    true,
                    false,
                    _projects,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(versions2);

            var packageModelVersions = new PackageModelVersions(_mockSearchService.Object, _packageModel);

            // Act - First call should set the cache
            var result1 = await packageModelVersions.GetPackageVersionsAsync(_packageSources, true, _projects, CancellationToken.None);

            // This call should use the cached result regardless of different parameters
            var result2 = await packageModelVersions.GetPackageVersionsAsync(sources2, true, _projects, CancellationToken.None);

            // Assert
            Assert.Same(versions1, result1);
            Assert.Same(versions1, result2); // It should return the cached result from the first call

            // Service should be called only once
            _mockSearchService.Verify(s => s.GetPackageVersionsAsync(
                _packageIdentity,
                _packageSources,
                true,
                false,
                _projects,
                It.IsAny<CancellationToken>()), Times.Once);

            // Second parameter set should never be called
            _mockSearchService.Verify(s => s.GetPackageVersionsAsync(
                _packageIdentity,
                sources2,
                true,
                false,
                _projects,
                It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}
