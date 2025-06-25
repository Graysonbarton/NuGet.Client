// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.Sdk.TestFramework;
using Microsoft.VisualStudio.Shell;
using Moq;
using NuGet.Configuration;
using NuGet.PackageManagement.VisualStudio.Options;
using NuGet.VisualStudio;
using Xunit;

namespace NuGet.PackageManagement.VisualStudio.Test.Options
{
    [Collection(MockedVS.Collection)]
    public class PackageSourceMappingPageTests : NuGetExternalSettingsProviderTests<PackageSourceMappingPage>
    {
        private IEnumerable<PackageSource> _packageSources;

        public PackageSourceMappingPageTests(GlobalServiceProvider sp)
        {
            sp.Reset();
            NuGetUIThreadHelper.SetCustomJoinableTaskFactory(ThreadHelper.JoinableTaskFactory);
            _packageSources = Enumerable.Empty<PackageSource>();
        }

        protected override PackageSourceMappingPage CreateInstance(VSSettings vsSettings)
        {
            Mock<IPackageSourceProvider> mockedPackageSourceProvider = new Mock<IPackageSourceProvider>();
            mockedPackageSourceProvider.Setup(packageSourceProvider => packageSourceProvider.LoadPackageSources())
                .Returns(_packageSources);

            PackageSourceMappingProvider sourceMappingProvider = new(vsSettings);

            return new PackageSourceMappingPage(vsSettings, mockedPackageSourceProvider.Object, sourceMappingProvider);
        }
    }
}
