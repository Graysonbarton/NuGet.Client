// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#nullable enable

using System;
using NuGet.PackageManagement.UI.Models;
using NuGet.VisualStudio.Internal.Contracts;

namespace NuGet.PackageManagement.UI
{
    public static class PackageModelFactory
    {
        public static PackageModel Create(PackageSearchMetadataContextInfo metadata, IVulnerable vulnerableCapability, IEmbeddedResources embeddedResources, IKnownOwnersCapable knownOwnersCapability)
        {
            if (metadata == null)
            {
                throw new ArgumentNullException(nameof(metadata));
            }

            if (metadata.PackagePath != null && metadata.TransitiveOrigins != null)
            {
                return new TransitivelyReferencedPackageModel(
                    metadata.Identity ?? throw new ArgumentNullException(nameof(metadata.Identity)),
                    metadata.PackagePath,
                    vulnerableCapability,
                    embeddedResources,
                    metadata.TransitiveOrigins,
                    metadata.Title,
                    metadata.Description,
                    metadata.Authors,
                    metadata.ProjectUrl,
                    metadata.Tags?.Split(','),
                    null, /*metadata.Copyright*/
                    metadata.OwnersList,
                    metadata.DependencySets,
                    metadata.Summary,
                    metadata.Published,
                    metadata.LicenseMetadata,
                    metadata.LicenseUrl,
                    metadata.RequireLicenseAcceptance,
                    metadata.ReportAbuseUrl?.ToString());
            }
            else if (metadata.PackagePath != null)
            {
                // installed and no transitive origins
                return new ReferencedPackageModel(
                    metadata.Identity ?? throw new ArgumentNullException(nameof(metadata.Identity)),
                    metadata.PackagePath,
                    vulnerableCapability,
                    embeddedResources,
                    metadata.Title,
                    metadata.Description,
                    metadata.Authors,
                    metadata.ProjectUrl,
                    metadata.Tags?.Split(','),
                    null, /*metadata.Copyright*/
                    metadata.OwnersList,
                    metadata.DependencySets,
                    metadata.Summary,
                    metadata.Published,
                    metadata.LicenseMetadata,
                    metadata.LicenseUrl,
                    metadata.RequireLicenseAcceptance,
                    metadata.ReportAbuseUrl?.ToString());
            }
            else if (metadata.PackagePath == null)
            {
                return new RemotePackageModel(
                    metadata.Identity ?? throw new ArgumentNullException(nameof(metadata.Identity)),
                    vulnerableCapability,
                    embeddedResources,
                    knownOwnersCapability,
                    metadata.Title,
                    metadata.Description,
                    metadata.Authors,
                    metadata.ProjectUrl,
                    metadata.Tags?.Split(','),
                    null, /*metadata.Copyright*/
                    metadata.OwnersList,
                    metadata.DependencySets,
                    metadata.Summary,
                    metadata.Published,
                    metadata.LicenseMetadata,
                    metadata.LicenseUrl,
                    metadata.RequireLicenseAcceptance,
                    metadata.IsListed,
                    metadata.PackageDetailsUrl,
                    metadata.DownloadCount,
                    metadata.ReadmeUrl);
            }
            else
            {
                return new LocalPackageModel(
                    metadata.Identity ?? throw new ArgumentNullException(nameof(metadata.Identity)),
                    metadata.PackagePath,
                    vulnerableCapability,
                    embeddedResources,
                    metadata.Title,
                    metadata.Description,
                    metadata.Authors,
                    metadata.ProjectUrl,
                    metadata.Tags?.Split(','),
                    null, /*metadata.Copyright*/
                    metadata.OwnersList,
                    metadata.DependencySets,
                    metadata.Summary,
                    metadata.Published,
                    metadata.LicenseMetadata,
                    metadata.LicenseUrl,
                    metadata.RequireLicenseAcceptance);
            }
        }
    }
}
