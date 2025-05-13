// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.Versioning;

namespace NuGet.PackageManagement.UI.Models.Package
{
    public abstract class PackageModel
    {
        internal PackageModel(PackageIdentity identity,
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
            Uri? iconUrl)
        {
            Identity = identity ?? throw new ArgumentNullException(nameof(identity));
            Title = title;
            Description = description;
            Authors = authors;
            ProjectUrl = projectUrl;
            Tags = tags;
            OwnersList = ownersList;
            Summary = summary;
            PublishedDate = publishedDate;
            LicenseMetadata = licenseMetadata;
            LicenseUrl = licenseUrl;
            RequireLicenseAcceptance = requireLicenseAcceptance;
            IconUrl = iconUrl;

            if (packageDependencyGroups != null && packageDependencyGroups.Count > 0)
            {
                DependencySets = [.. packageDependencyGroups.Select(e => new PackageDependencySetMetadata(e))];
            }
        }

        public PackageIdentity Identity { get; }

        public string Id => Identity.Id;

        public NuGetVersion Version => Identity.Version;

        public string? Title { get; }

        public string? Description { get; }

        public string? Authors { get; }

        public IReadOnlyList<string>? OwnersList { get; }

        public IReadOnlyCollection<PackageDependencySetMetadata>? DependencySets { get; }

        public Uri? ProjectUrl { get; }

        public string[]? Tags { get; }

        public string? Summary { get; }

        public LicenseMetadata? LicenseMetadata { get; }

        public Uri? LicenseUrl { get; }

        public bool RequireLicenseAcceptance { get; }

        public DateTimeOffset? PublishedDate { get; }

        public Uri? IconUrl { get; }

        public abstract Task PopulateDataAsync(CancellationToken cancellationToken);
    }
}
