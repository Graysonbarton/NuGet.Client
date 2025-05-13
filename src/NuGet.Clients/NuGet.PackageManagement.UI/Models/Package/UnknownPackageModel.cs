// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#nullable enable

using System.Threading;
using System.Threading.Tasks;
using NuGet.Packaging.Core;

namespace NuGet.PackageManagement.UI.Models.Package
{
    internal class UnknownPackageModel : PackageModel
    {
        public UnknownPackageModel(PackageIdentity identity)
          : base(identity, title: null, description: null, authors: null, projectUrl: null, tags: null,
                ownersList: null, packageDependencyGroups: null, summary: null, publishedDate: null,
                licenseMetadata: null, licenseUrl: null, requireLicenseAcceptance: false, iconUrl: null)
        {
        }

        public override Task PopulateDataAsync(CancellationToken cancellationToken)
        {
            // LocalPackageModel does not need to populate any additional data.
            return Task.CompletedTask;
        }
    }
}
