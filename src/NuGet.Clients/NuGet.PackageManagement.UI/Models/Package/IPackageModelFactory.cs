// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#nullable enable

using NuGet.VisualStudio.Internal.Contracts;
using ContractItemFilter = NuGet.VisualStudio.Internal.Contracts.ItemFilter;

namespace NuGet.PackageManagement.UI.Models.Package
{
    public interface IPackageModelFactory
    {
        PackageModel Create(string identity, VersionInfoContextInfo version);
        PackageModel Create(PackageSearchMetadataContextInfo metadata, ContractItemFilter itemFilter);
    }
}
