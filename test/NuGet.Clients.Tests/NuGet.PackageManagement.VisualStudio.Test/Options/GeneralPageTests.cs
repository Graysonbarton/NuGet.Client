// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using NuGet.PackageManagement.VisualStudio.Options;

namespace NuGet.PackageManagement.VisualStudio.Test.Options
{
    public class GeneralPageTests : NuGetExternalSettingsProviderTests<GeneralPage>
    {
        protected override GeneralPage CreateInstance(VSSettings vsSettings)
        {
            return new GeneralPage(vsSettings);
        }

        protected override List<string> GetMonikers()
        {
            return new List<string>
            {
                GeneralPage.MonikerAllowRestoreDownload
            };
        }
    }
}
