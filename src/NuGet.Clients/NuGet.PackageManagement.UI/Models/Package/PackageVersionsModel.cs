// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NuGet.VisualStudio.Internal.Contracts;

namespace NuGet.PackageManagement.UI.Models.Package
{
    public class PackageVersionsModel
    {
        private INuGetSearchService _nuGetSearchService;
        private IReadOnlyCollection<VersionInfoContextInfo>? _availableVersions;
        private PackageModel _primaryPackage;
        private bool _dataLoaded;

        public PackageVersionsModel(
            INuGetSearchService nuGetSearchService,
            PackageModel basePackage)
        {
            _nuGetSearchService = nuGetSearchService ?? throw new ArgumentException(nameof(nuGetSearchService));
            _primaryPackage = basePackage ?? throw new ArgumentNullException(nameof(basePackage));
            _dataLoaded = false;
        }

        public string Id => _primaryPackage.Id;

        public IReadOnlyCollection<VersionInfoContextInfo>? Versions => _availableVersions;

        public async Task PopulateDataAsync(IReadOnlyCollection<PackageSourceContextInfo> packageSources, bool includePrelease, IEnumerable<IProjectContextInfo> projects, CancellationToken cancellationToken)
        {
            if (!_dataLoaded)
            {
                _dataLoaded = true;
                var isTransitive = _primaryPackage is TransitivelyReferencedPackageModel;
                _availableVersions = await _nuGetSearchService.GetPackageVersionsAsync(_primaryPackage.Identity, packageSources, includePrelease, isTransitive, projects, cancellationToken);
            }
        }
    }
}
