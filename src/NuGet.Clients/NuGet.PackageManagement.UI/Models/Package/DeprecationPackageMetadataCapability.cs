// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#nullable enable

using System;
using System.Threading;
using System.Threading.Tasks;

namespace NuGet.PackageManagement.UI.Models.Package
{
    internal class DeprecationPackageMetadataCapability : DeprecationCapabilityBase
    {
        private readonly IPackageMetadataRetrievalAdapter _packageMetadataRetrievalAdapter;

        public DeprecationPackageMetadataCapability(IPackageMetadataRetrievalAdapter packageMetadataRetrievalAdapter)
        {
            _packageMetadataRetrievalAdapter = packageMetadataRetrievalAdapter ?? throw new ArgumentNullException(nameof(packageMetadataRetrievalAdapter));
        }

        public override async Task PopulateDataAsync(CancellationToken cancellationToken)
        {
            _deprecationMetadata = await _packageMetadataRetrievalAdapter.GetPackageDeprecationInfoAsync(cancellationToken);
        }
    }
}
