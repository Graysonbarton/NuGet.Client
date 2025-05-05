// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using NuGet.PackageManagement.UI.Models.Package;
using NuGet.VisualStudio.Internal.Contracts;

namespace NuGet.PackageManagement.UI.Test.Models.Package
{
    internal class TestDeprecationCapability : DeprecationCapabilityBase
    {
        public TestDeprecationCapability(PackageDeprecationMetadataContextInfo packageDeprecationMetadataContextInfo)
        {
            _deprecationMetadata = packageDeprecationMetadataContextInfo;
        }

        public override Task PopulateDataAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
