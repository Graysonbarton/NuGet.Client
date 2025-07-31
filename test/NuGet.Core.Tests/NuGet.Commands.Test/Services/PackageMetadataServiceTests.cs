// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using NuGet.Common;
using NuGet.Commands.Experimental.Services;
using NuGet.Configuration;
using NuGet.Protocol.Core.Types;
using Xunit;
using NuGet.Versioning;
using NuGet.Packaging.Core;

namespace NuGet.Commands.Test.Services
{
    public class PackageMetadataServiceTests
    {
        private readonly Mock<ILogger> _loggerMock = new();

        [Fact]
        public async Task GetLatestMetadata_ThrowsArgumentException_WhenPackageIdIsNullOrWhitespace()
        {
            // Arrange
            PackageMetadataService service = new(_loggerMock.Object);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => service.GetLatestMetadataAsync(null, [new PackageSource("source")]));
            await Assert.ThrowsAsync<ArgumentException>(() => service.GetLatestMetadataAsync("", [new PackageSource("source")]));
            await Assert.ThrowsAsync<ArgumentException>(() => service.GetLatestMetadataAsync("   ", [new PackageSource("source")]));
        }

        [Fact]
        public async Task GetLatestMetadata_ThrowsArgumentNullException_WhenPackageSourcesIsNull()
        {
            // Arrange
            PackageMetadataService service = new(_loggerMock.Object);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => service.GetLatestMetadataAsync("packageId", null));
        }

        [Fact]
        public async Task GetLatestMetadata_ThrowsArgumentException_WhenPackageSourcesIsEmpty()
        {
            // Arrange
            PackageMetadataService service = new(_loggerMock.Object);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => service.GetLatestMetadataAsync("packageId", []));
        }

        [Fact]
        public async Task GetLatestMetadata_ReturnsLatestVersion_WhenMultipleSources()
        {
            // Arrange
            PackageSource source1 = new("https://source1", "source1");
            PackageSource source2 = new("https://source2", "source2");

            Mock<PackageMetadataResource> resource1 = CreatePackageMetadataResourceMock("Contoso.A", "1.0.0", "2.0.0");
            Mock<PackageMetadataResource> resource2 = CreatePackageMetadataResourceMock("Contoso.A", "1.0.0", "3.0.0");

            Mock<INuGetResourceProvider> resourceProvider = new();
            SetupResourceProviderForSource(resourceProvider, source1, resource1.Object);
            SetupResourceProviderForSource(resourceProvider, source2, resource2.Object);
            List<Lazy<INuGetResourceProvider>> providers = [new(() => resourceProvider.Object)];
            PackageMetadataService service = new(providers, _loggerMock.Object);

            // Act
            PackageMetadataSourceResult result = await service.GetLatestMetadataAsync("Contoso.A", [source1, source2]);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("3.0.0", result.Metadata.Identity.Version.ToFullString());
            Assert.Equal(source2, result.Source);
        }

        [Fact]
        public async Task GetLatestMetadata_ReturnsNull_WhenPackageNotFoundInSources()
        {
            // Arrange
            PackageSource source1 = new("https://source1", "source1");
            PackageSource source2 = new("https://source2", "source2");

            Mock<PackageMetadataResource> resource1 = CreatePackageMetadataResourceMock("Contoso.A", "1.0.0", "2.0.0");
            Mock<PackageMetadataResource> resource2 = CreatePackageMetadataResourceMock("Contoso.A", "1.0.0", "3.0.0");

            Mock<INuGetResourceProvider> resourceProvider = new();
            SetupResourceProviderForSource(resourceProvider, source1, resource1.Object);
            SetupResourceProviderForSource(resourceProvider, source2, resource2.Object);
            List<Lazy<INuGetResourceProvider>> providers = [new(() => resourceProvider.Object)];
            PackageMetadataService service = new(providers, _loggerMock.Object);

            // Act
            PackageMetadataSourceResult result = await service.GetLatestMetadataAsync("Contoso.B", [source1, source2]);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetLatestMetadata_ReturnsLatestVersion_WhenSourcesReturnOverlappingVersions()
        {
            // Arrange
            PackageSource overlappingSource1 = new("https://overlap1", "overlap1");
            PackageSource overlappingSource2 = new("https://overlap2", "overlap2");

            Mock<PackageMetadataResource> resource1 = CreatePackageMetadataResourceMock("Contoso.Overlap", "1.0.0", "2.0.0", "3.0.0");
            Mock<PackageMetadataResource> resource2 = CreatePackageMetadataResourceMock("Contoso.Overlap", "2.0.0", "3.0.0", "4.0.0");

            Mock<INuGetResourceProvider> resourceProvider = new();
            SetupResourceProviderForSource(resourceProvider, overlappingSource1, resource1.Object);
            SetupResourceProviderForSource(resourceProvider, overlappingSource2, resource2.Object);
            List<Lazy<INuGetResourceProvider>> providers = [new(() => resourceProvider.Object)];
            PackageMetadataService service = new(providers, _loggerMock.Object);

            // Act
            PackageMetadataSourceResult result = await service.GetLatestMetadataAsync("Contoso.Overlap", [overlappingSource1, overlappingSource2]);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("4.0.0", result.Metadata.Identity.Version.ToFullString());
            Assert.Equal(overlappingSource2, result.Source);
        }

        [Fact]
        public async Task GetLatestMetadata_ReturnsLatestVersion_WhenSourcesReturnPrereleaseVersions()
        {
            // Arrange
            PackageSource preSource1 = new("https://pre1", "pre1");
            PackageSource preSource2 = new("https://pre2", "pre2");

            Mock<PackageMetadataResource> resource1 = CreatePackageMetadataResourceMock("Contoso.Pre", "1.0.0", "2.0.0-beta", "2.0.0");
            Mock<PackageMetadataResource> resource2 = CreatePackageMetadataResourceMock("Contoso.Pre", "1.5.0-alpha", "2.1.0-rc");

            Mock<INuGetResourceProvider> resourceProvider = new();
            SetupResourceProviderForSource(resourceProvider, preSource1, resource1.Object);
            SetupResourceProviderForSource(resourceProvider, preSource2, resource2.Object);
            List<Lazy<INuGetResourceProvider>> providers = [new(() => resourceProvider.Object)];
            PackageMetadataService service = new(providers, _loggerMock.Object);

            // Act
            PackageMetadataSourceResult result = await service.GetLatestMetadataAsync("Contoso.Pre", [preSource1, preSource2]);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("2.1.0-rc", result.Metadata.Identity.Version.ToFullString());
            Assert.Equal(preSource2, result.Source);
        }

        [Fact]
        public async Task GetLatestMetadata_ReturnsNull_WhenSourceReturnEmptyMetadata()
        {
            // Arrange
            PackageSource emptySource = new("https://empty", "empty");

            Mock<PackageMetadataResource> resource = new();
            resource.Setup(x => x.GetMetadataAsync(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<SourceCacheContext>(), It.IsAny<ILogger>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync([]);

            Mock<INuGetResourceProvider> resourceProvider = new();
            SetupResourceProviderForSource(resourceProvider, emptySource, resource.Object);
            List<Lazy<INuGetResourceProvider>> providers = [new(() => resourceProvider.Object)];
            PackageMetadataService service = new(providers, _loggerMock.Object);

            // Act
            PackageMetadataSourceResult result = await service.GetLatestMetadataAsync("Contoso.A", [emptySource]);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetLatestMetadata_ThrowsOperationCanceledException_WhenCancellationRequested()
        {
            // Arrange
            PackageSource source = new("https://source1", "Source1");

            CancellationTokenSource cts = new();
            Mock<PackageMetadataResource> resource = new();
            resource.Setup(x => x.GetMetadataAsync(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<SourceCacheContext>(), It.IsAny<ILogger>(), It.IsAny<CancellationToken>()))
                .Returns<string, bool, bool, SourceCacheContext, ILogger, CancellationToken>((_, _, _, _, _, token) =>
                {
                    token.ThrowIfCancellationRequested();
                    return Task.FromResult<IEnumerable<IPackageSearchMetadata>>([]);
                });

            Mock<INuGetResourceProvider> resourceProvider = new();
            SetupResourceProviderForSource(resourceProvider, source, resource.Object);
            List<Lazy<INuGetResourceProvider>> providers = [new(() => resourceProvider.Object)];

            PackageMetadataService service = new(providers, _loggerMock.Object);
            cts.Cancel();

            // Act & Assert
            await Assert.ThrowsAsync<OperationCanceledException>(() => service.GetLatestMetadataAsync("Contoso.Cancel", [source], cancellationToken: cts.Token));
        }

        [Fact]
        public async Task GetLatestMetadata_ReturnsLatestVersion_WhenSourcesReturnVersionsWithMetadataSuffix()
        {
            // Arrange
            PackageSource metaSource = new("https://meta", "meta");

            Mock<PackageMetadataResource> resource = CreatePackageMetadataResourceMock("Contoso.Meta", "1.0.0+build.1", "1.0.0", "2.0.0+meta");

            Mock<INuGetResourceProvider> resourceProvider = new();
            SetupResourceProviderForSource(resourceProvider, metaSource, resource.Object);
            List<Lazy<INuGetResourceProvider>> providers = [new(() => resourceProvider.Object)];
            PackageMetadataService service = new(providers, _loggerMock.Object);

            // Act
            PackageMetadataSourceResult result = await service.GetLatestMetadataAsync("Contoso.Meta", [metaSource]);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("2.0.0+meta", result.Metadata.Identity.Version.ToFullString());
        }

        private static void SetupResourceProviderForSource(Mock<INuGetResourceProvider> resourceProvider, PackageSource source, PackageMetadataResource packageMetadataResource)
        {
            resourceProvider.Setup(x => x.TryCreate(It.Is<SourceRepository>(repo => repo.PackageSource.Name == source.Name), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Tuple<bool, INuGetResource?>(true, packageMetadataResource));

            resourceProvider.SetupGet(x => x.ResourceType).Returns(typeof(PackageMetadataResource));
        }

        private static Mock<PackageMetadataResource> CreatePackageMetadataResourceMock(string packageId, params string[] versions)
        {
            var packageMetadataResource = new Mock<PackageMetadataResource>();
            packageMetadataResource.Setup(x => x.GetMetadataAsync(It.Is<string>(pId => pId == packageId), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<SourceCacheContext>(), It.IsAny<ILogger>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((string _, bool _, bool _, SourceCacheContext _, ILogger _, CancellationToken cancellationToken) =>
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    return versions.Select(version => new PackageSearchMetadataBuilder.ClonedPackageSearchMetadata()
                    {
                        Identity = new PackageIdentity(packageId, NuGetVersion.Parse(version))
                    }).ToList();
                });

            return packageMetadataResource;
        }
    }
}
