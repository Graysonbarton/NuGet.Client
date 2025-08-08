// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#nullable enable

using NuGet.Configuration;
using NuGet.Protocol.Core.Types;

namespace NuGet.Commands.Experimental.Services
{
    public record PackageMetadataSourceResult
    {
        public required IPackageSearchMetadata Metadata { get; init; }
        public required PackageSource Source { get; init; }
    }
}
