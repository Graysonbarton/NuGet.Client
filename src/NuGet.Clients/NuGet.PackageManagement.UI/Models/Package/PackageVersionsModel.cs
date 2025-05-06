// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NuGet.Packaging.Core;
using NuGet.VisualStudio.Internal.Contracts;

namespace NuGet.PackageManagement.UI.Models.Package
{
    public class PackageVersionsModel
    {
        private INuGetSearchService _nuGetSearchService;
        private IReadOnlyCollection<VersionInfoContextInfo>? _availableVersions;
        private bool _dataLoaded;

        public PackageVersionsModel(
            PackageIdentity packageIdentity,
            INuGetSearchService nuGetSearchService)
        {
            _nuGetSearchService = nuGetSearchService ?? throw new ArgumentException(nameof(nuGetSearchService));
            _dataLoaded = false;
            Id = packageIdentity ?? throw new ArgumentNullException(nameof(packageIdentity));
        }

        public PackageIdentity Id { get; private set; }

        public IReadOnlyCollection<VersionInfoContextInfo>? Versions => _availableVersions;

        public async Task PopulateDataAsync(IReadOnlyCollection<PackageSourceContextInfo> packageSources, bool includePrelease, bool isTransitive, IEnumerable<IProjectContextInfo> projects, CancellationToken cancellationToken)
        {
            if (!_dataLoaded)
            {
                _dataLoaded = true;
                _availableVersions = await _nuGetSearchService.GetPackageVersionsAsync(Id, packageSources, includePrelease, isTransitive, projects, cancellationToken);
            }
        }
    }
}
