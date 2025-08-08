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
        public async Task GetLatestMetadata_WhenPackageIdIsNullOrWhitespace_ThrowsArgumentException()
        {
            // Arrange
            PackageMetadataService service = new(_loggerMock.Object);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => service.GetLatestMetadataAsync(null!, [new PackageSource("source")]));
            await Assert.ThrowsAsync<ArgumentException>(() => service.GetLatestMetadataAsync("", [new PackageSource("source")]));
            await Assert.ThrowsAsync<ArgumentException>(() => service.GetLatestMetadataAsync("   ", [new PackageSource("source")]));
        }

        [Fact]
        public async Task GetLatestMetadata_WhenPackageSourcesIsNull_ThrowsArgumentNullException()
        {
            // Arrange
            PackageMetadataService service = new(_loggerMock.Object);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => service.GetLatestMetadataAsync("packageId", null!));
        }

        [Fact]
        public async Task GetLatestMetadata_WhenPackageSourcesIsEmpty_ThrowsArgumentException()
        {
            // Arrange
            PackageMetadataService service = new(_loggerMock.Object);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => service.GetLatestMetadataAsync("packageId", []));
        }

        [Fact]
        public async Task GetLatestMetadata_WhenMultipleSources_ReturnsLatestVersion()
        {
            // Arrange
            PackageSource source1 = new("https://source1.test", "source1");
            PackageSource source2 = new("https://source2.test", "source2");

            Mock<PackageMetadataResource> resource1 = CreatePackageMetadataResourceMock("Contoso.A", "1.0.0", "2.0.0");
            Mock<PackageMetadataResource> resource2 = CreatePackageMetadataResourceMock("Contoso.A", "1.0.0", "3.0.0");

            Mock<INuGetResourceProvider> resourceProvider = new();
            SetupResourceProviderForSource(resourceProvider, source1, resource1.Object);
            SetupResourceProviderForSource(resourceProvider, source2, resource2.Object);
            List<Lazy<INuGetResourceProvider>> providers = [new(() => resourceProvider.Object)];
            PackageMetadataService service = new(providers, _loggerMock.Object);

            // Act
            PackageMetadataSourceResult? result = await service.GetLatestMetadataAsync("Contoso.A", [source1, source2]);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("3.0.0", result.Metadata.Identity.Version.ToFullString());
            Assert.Equal(source2, result.Source);
        }

        [Fact]
        public async Task GetLatestMetadata_WhenPackageNotFoundInSources_ReturnsNull()
        {
            // Arrange
            PackageSource source = new("https://source1.test", "source1");

            Mock<PackageMetadataResource> resource = CreatePackageMetadataResourceMock("Contoso.A", "1.0.0", "2.0.0");

            Mock<INuGetResourceProvider> resourceProvider = new();
            SetupResourceProviderForSource(resourceProvider, source, resource.Object);
            List<Lazy<INuGetResourceProvider>> providers = [new(() => resourceProvider.Object)];
            PackageMetadataService service = new(providers, _loggerMock.Object);

            // Act
            PackageMetadataSourceResult? result = await service.GetLatestMetadataAsync("Contoso.B", [source]);

            // Assert
            Assert.Null(result);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task GetLatestMetadata_WhenCalledWithIncludePrerelease_PassesPrereleaseParameterToResource(bool includePrerelease)
        {
            // Arrange
            PackageSource source = new("https://prerelease.test", "prerelease");

            bool? receivedIncludePrerelease = null;

            Mock<PackageMetadataResource> resource = new();
            resource.Setup(x => x.GetMetadataAsync(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<SourceCacheContext>(), It.IsAny<ILogger>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((string packageId, bool includePrerelease, bool includeUnlisted, SourceCacheContext cache, ILogger logger, CancellationToken token) =>
                {
                    receivedIncludePrerelease = includePrerelease;
                    return [];
                });

            var resourceProvider = new Mock<INuGetResourceProvider>();
            SetupResourceProviderForSource(resourceProvider, source, resource.Object);
            List<Lazy<INuGetResourceProvider>> providers = [new(() => resourceProvider.Object)];
            PackageMetadataService service = new(providers, _loggerMock.Object);

            // Act
            await service.GetLatestMetadataAsync("Contoso.Prerelease", [source], includePrerelease: includePrerelease);

            // Assert
            Assert.True(receivedIncludePrerelease.HasValue);
            Assert.Equal(receivedIncludePrerelease.Value, includePrerelease);
        }

        [Fact]
        public async Task GetLatestMetadata_WhenSourceReturnEmptyMetadata_ReturnsNull()
        {
            // Arrange
            PackageSource emptySource = new("https://empty.test", "empty");

            Mock<PackageMetadataResource> resource = new();
            resource.Setup(x => x.GetMetadataAsync(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<SourceCacheContext>(), It.IsAny<ILogger>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync([]);

            Mock<INuGetResourceProvider> resourceProvider = new();
            SetupResourceProviderForSource(resourceProvider, emptySource, resource.Object);
            List<Lazy<INuGetResourceProvider>> providers = [new(() => resourceProvider.Object)];
            PackageMetadataService service = new(providers, _loggerMock.Object);

            // Act
            PackageMetadataSourceResult? result = await service.GetLatestMetadataAsync("Contoso.A", [emptySource]);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetLatestMetadata_WhenCancellationRequested_ThrowsOperationCanceledException()
        {
            // Arrange
            PackageSource source = new("https://source1.test", "Source1");

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
