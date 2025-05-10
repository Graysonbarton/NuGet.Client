// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NuGet.Packaging.Core;
using NuGet.Versioning;
using NuGet.VisualStudio.Internal.Contracts;

namespace NuGet.PackageManagement.UI.Models.Package
{
    public class PackageVersionsModel
    {
        private INuGetSearchService _nuGetSearchService;
        private IReadOnlyCollection<VersionInfoContextInfo>? _availableVersions;
        private IReadOnlyCollection<PackageSourceContextInfo> _packageSources;
        private bool _hasDataLoaded;
        private bool _includePrelease;

        public PackageVersionsModel(
            IReadOnlyCollection<PackageSourceContextInfo> packageSources,
            bool includePrelease,
            PackageIdentity packageIdentity,
            INuGetSearchService nuGetSearchService)
        {
            _includePrelease = includePrelease;
            _packageSources = packageSources ?? throw new ArgumentNullException(nameof(packageSources));
            _nuGetSearchService = nuGetSearchService ?? throw new ArgumentNullException(nameof(nuGetSearchService));
            _hasDataLoaded = false;
            Id = packageIdentity ?? throw new ArgumentNullException(nameof(packageIdentity));
        }

        public PackageIdentity Id { get; private set; }

        public IReadOnlyCollection<VersionInfoContextInfo>? Versions => _availableVersions;

        public NuGetVersion? GetLatestVersion(VersionRange allowedVersions)
        {
            return _availableVersions?.Where(v => allowedVersions.Satisfies(v.Version)).Max(v => v.Version);
        }

        public async Task PopulateDataAsync(CancellationToken cancellationToken)
        {
            if (!_hasDataLoaded)
            {
                _availableVersions = await _nuGetSearchService.GetPackageVersionsAsync(Id, _packageSources, _includePrelease, cancellationToken);
                _hasDataLoaded = true;
            }
        }
    }
}
