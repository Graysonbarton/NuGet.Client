// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NuGet.Configuration;

namespace NuGet.Commands.Experimental.Services
{
    public interface IPackageMetadataService
    {
        Task<PackageMetadataSourceResult> GetLatestMetadataAsync(string packageId, IEnumerable<PackageSource> packageSources, bool includePrerelease = false, CancellationToken cancellationToken = default);
    }
}
