// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using NuGet.Configuration;
using NuGet.Protocol.Core.Types;

namespace NuGet.Commands.Experimental.Services
{
    public class PackageMetadataSourceResult
    {
        public PackageMetadataSourceResult(IPackageSearchMetadata metadata, PackageSource source)
        {
            Metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            Source = source ?? throw new ArgumentNullException(nameof(source));
        }

        public IPackageSearchMetadata Metadata { get; }
        public PackageSource Source { get; }
    }
}
