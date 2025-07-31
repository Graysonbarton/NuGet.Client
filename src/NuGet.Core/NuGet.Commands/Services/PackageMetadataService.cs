// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NuGet.Common;
using NuGet.Configuration;
using NuGet.Protocol.Core.Types;

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

        public async Task<PackageMetadataSourceResult> GetLatestMetadataAsync(string packageId, IEnumerable<PackageSource> packageSources, bool includePrerelease = false, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(packageId))
            {
                throw new ArgumentException("Package ID must be provided.", nameof(packageId));
            }

            if (packageSources is null)
            {
                throw new ArgumentNullException(nameof(packageSources));
            }

            if (!packageSources.Any())
            {
                throw new ArgumentException("Package Sources must be provided.", nameof(packageSources));
            }

            IEnumerable<PackageMetadataSourceResult> latestMetadataPerSource = await GetLatestMetadataPerSourceAsync(packageId, packageSources, includePrerelease, cancellationToken);
            return latestMetadataPerSource?.OrderByDescending(m => m.Metadata.Identity.Version).FirstOrDefault();
        }

        private async Task<IEnumerable<PackageMetadataSourceResult>> GetLatestMetadataPerSourceAsync(string packageId, IEnumerable<PackageSource> packageSources, bool includePrerelease = false, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(packageId) || packageSources == null || !packageSources.Any())
            {
                return null;
            }

            List<Task<PackageMetadataSourceResult>> tasks = packageSources.Select(async source =>
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
                    metadataResults = await metadataResource.GetMetadataAsync(
                        packageId,
                        includePrerelease: includePrerelease,
                        includeUnlisted: false,
                        sourceCacheContext,
                        _logger,
                        cancellationToken);
                }

                if (metadataResults == null || !metadataResults.Any())
                {
                    return null;
                }

                IPackageSearchMetadata latestPackage = metadataResults
                    .OrderByDescending(p => p.Identity.Version)
                    .FirstOrDefault();
                return new PackageMetadataSourceResult(latestPackage, source);
            }).ToList();

            List<PackageMetadataSourceResult> results = (await Task.WhenAll(tasks))
                .Where(r => r != null)
                .ToList();

            return results;
        }
    }
}
