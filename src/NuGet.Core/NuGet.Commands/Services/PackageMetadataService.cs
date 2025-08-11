// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NuGet.Common;
using NuGet.Configuration;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;

namespace NuGet.Commands.Experimental.Services
{
    public class PackageMetadataService : IPackageMetadataService
    {
        private readonly ILogger _logger;
        private readonly IEnumerable<Lazy<INuGetResourceProvider>> _resourceProviders;

        public PackageMetadataService(IEnumerable<Lazy<INuGetResourceProvider>> resourceProviders, ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _resourceProviders = resourceProviders ?? throw new ArgumentNullException(nameof(resourceProviders));
        }

        public PackageMetadataService(ILogger logger)
            : this(Repository.Provider.GetCoreV3(), logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<PackageMetadataSourceResult?> GetLatestMetadataAsync(string packageId, IReadOnlyCollection<PackageSource> packageSources, bool includePrerelease = false, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(packageId))
            {
                throw new ArgumentException("Package ID must be provided.", nameof(packageId));
            }

            if (packageSources is null)
            {
                throw new ArgumentNullException(nameof(packageSources));
            }

            if (packageSources.Count == 0)
            {
                throw new ArgumentException("Package Sources must be provided.", nameof(packageSources));
            }

            IReadOnlyCollection<PackageMetadataSourceResult>? latestMetadataPerSource = await GetLatestMetadataPerSourceAsync(packageId, packageSources, includePrerelease, cancellationToken);
            if (latestMetadataPerSource is null)
            {
                return null;
            }

            return GetLatestOrDefault(latestMetadataPerSource, (x, y) => VersionComparer.VersionReleaseMetadata.Compare(x.Metadata.Identity.Version, y.Metadata.Identity.Version) > 0);
        }

        private async Task<IReadOnlyCollection<PackageMetadataSourceResult>?> GetLatestMetadataPerSourceAsync(string packageId, IEnumerable<PackageSource> packageSources, bool includePrerelease, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(packageId) || packageSources == null || !packageSources.Any())
            {
                return null;
            }

            List<Task<PackageMetadataSourceResult?>> tasks = packageSources.Select(async source =>
            {
                SourceRepository repository = new SourceRepository(source, _resourceProviders);
                var metadataResource = await repository.GetResourceAsync<PackageMetadataResource>(cancellationToken);
                if (metadataResource == null)
                {
                    return null;
                }

                IEnumerable<IPackageSearchMetadata> metadataResults;
                using (var sourceCacheContext = new SourceCacheContext())
                {
                    metadataResults = (await metadataResource.GetMetadataAsync(
                        packageId,
                        includePrerelease: includePrerelease,
                        includeUnlisted: false,
                        sourceCacheContext,
                        _logger,
                        cancellationToken));
                }

                if (metadataResults is null || !metadataResults.Any())
                {
                    return null;
                }

                IPackageSearchMetadata? latestPackage = GetLatestOrDefault(metadataResults, (x, y) => VersionComparer.VersionReleaseMetadata.Compare(x.Identity.Version, y.Identity.Version) > 0);
                if (latestPackage is null)
                {
                    return null;
                }

                return new PackageMetadataSourceResult() { Metadata = latestPackage, Source = source };
            }).ToList();

            List<PackageMetadataSourceResult> results = (await Task.WhenAll(tasks))
                .Where(r => r is not null && r.Metadata is not null)
                .Select(r => r!)
                .ToList();

            return results;
        }
        private static T? GetLatestOrDefault<T>(IEnumerable<T> items, Func<T, T, bool> isLater)
        {
            T? latest = default;
            foreach (var item in items)
            {
                if (item == null)
                {
                    continue;
                }

                if (latest is null || isLater(item, latest))
                {
                    latest = item;
                }
            }
            return latest;
        }
    }
}
