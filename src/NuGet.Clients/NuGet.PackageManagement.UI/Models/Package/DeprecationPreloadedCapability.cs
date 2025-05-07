// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#nullable enable

using System.Threading;
using System.Threading.Tasks;
using NuGet.VisualStudio.Internal.Contracts;

namespace NuGet.PackageManagement.UI.Models.Package
{
    internal class DeprecationPreloadedCapability : DeprecationCapabilityBase
    {
        public DeprecationPreloadedCapability(PackageDeprecationMetadataContextInfo? deprecationMetadata)
        {
            _deprecationMetadata = deprecationMetadata;
        }

        public override Task PopulateDataAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
