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
    public class PackageModelVersions
    {
        private INuGetSearchService _nuGetSearchService;
        private IReadOnlyCollection<VersionInfoContextInfo>? _availableVersions;
        private PackageModel _basePackage;

        public PackageModelVersions(
            INuGetSearchService nuGetSearchService,
            PackageModel basePackage)
        {
            _nuGetSearchService = nuGetSearchService ?? throw new ArgumentException(nameof(nuGetSearchService));
            _basePackage = basePackage ?? throw new ArgumentNullException(nameof(basePackage));
        }

        public async Task<IReadOnlyCollection<VersionInfoContextInfo>> GetPackageVersionsAsync(IReadOnlyCollection<PackageSourceContextInfo> packageSources, bool includePrelease, IEnumerable<IProjectContextInfo> projects, CancellationToken cancellationToken)
        {
            if (_availableVersions == null)
            {
                var isTransitive = _basePackage is TransitivelyReferencedPackageModel;
                _availableVersions = await _nuGetSearchService.GetPackageVersionsAsync(_basePackage.Identity, packageSources, includePrelease, isTransitive, projects, cancellationToken);
            }
            return _availableVersions;
        }
    }
}
