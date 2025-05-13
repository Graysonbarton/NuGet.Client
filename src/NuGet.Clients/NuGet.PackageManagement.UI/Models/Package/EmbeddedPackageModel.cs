// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using NuGet.Packaging;
using NuGet.Packaging.Core;

namespace NuGet.PackageManagement.UI.Models.Package
{
    public abstract class EmbeddedPackageModel : PackageModel, IEmbeddedResourcesCapable
    {
        private readonly IEmbeddedResourcesCapable _embeddedResources;

        internal EmbeddedPackageModel(PackageIdentity identity,
            IEmbeddedResourcesCapable embeddedResources,
            string? title,
            string? description,
            string? authors,
            Uri? projectUrl,
            string[]? tags,
            IReadOnlyList<string>? ownersList,
            IReadOnlyCollection<PackageDependencyGroup>? packageDependencyGroups,
            string? summary,
            DateTimeOffset? publishedDate,
            LicenseMetadata? licenseMetadata,
            Uri? licenseUrl,
            bool requireLicenseAcceptance,
            Uri? iconUrl) : base(identity, title, description, authors, projectUrl, tags, ownersList, packageDependencyGroups, summary, publishedDate, licenseMetadata, licenseUrl, requireLicenseAcceptance, iconUrl)
        {
            _embeddedResources = embeddedResources ?? throw new ArgumentNullException(nameof(embeddedResources));
        }

        public Uri? ReadmeUri => _embeddedResources.ReadmeUri;

        public ValueTask<Stream?> GetIconAsync(CancellationToken cancellationToken) => _embeddedResources.GetIconAsync(cancellationToken);

        public ValueTask<Stream?> GetLicenseAsync(CancellationToken cancellationToken) => _embeddedResources.GetLicenseAsync(cancellationToken);

        public ValueTask<Stream?> GetReadmeAsync(CancellationToken cancellationToken) => _embeddedResources.GetReadmeAsync(cancellationToken);
    }
}
