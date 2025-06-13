// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#nullable enable

using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Utilities.UnifiedSettings;

namespace NuGet.PackageManagement.VisualStudio.Options
{
    [Guid("ACE317DA-8399-4DA4-9828-107E02244D45")]
    public class PackageSourceMappingPage : NuGetExternalSettingsProvider
    {
        public PackageSourceMappingPage(VSSettings vsSettings)
            : base(vsSettings)
        {
        }

        public override Task<ExternalSettingOperationResult<T>> GetValueAsync<T>(string moniker, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public override Task<ExternalSettingOperationResult> SetValueAsync<T>(string moniker, T value, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
