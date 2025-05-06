// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using NuGet.PackageManagement.VisualStudio.Options;
using Xunit;

namespace NuGet.PackageManagement.VisualStudio.Test.Options
{
    public class GeneralPageTests : NuGetExternalSettingsProviderTests<GeneralPage>
    {
        [Fact]
        public override Task GetValueAsync_MonikerDoesNotExist_ThrowsInvalidOperationExceptionAsync()
        {
            throw new System.NotImplementedException();
        }

        public override Task GetValueAsync_MonikerExists_ReturnsSuccessExternalSettingOperationResultAsync()
        {
            throw new System.NotImplementedException();
        }

        public override Task GetValueAsync_ValueInvalidForMoniker_ThrowsIncompatibleSettingTypeExceptionAsync()
        {
            throw new System.NotImplementedException();
        }

        public override Task SetValueAsync_MonikerDoesNotExist_InvalidOperationExceptionAsync()
        {
            throw new System.NotImplementedException();
        }

        public override Task SetValueAsync_MonikerExists_ReturnsSuccessExternalSettingOperationResultAsync()
        {
            throw new System.NotImplementedException();
        }

        public override Task SetValueAsync_ValueInvalidForMoniker_ThrowsIncompatibleSettingTypeExceptionAsync()
        {
            throw new System.NotImplementedException();
        }

        protected override GeneralPage CreateInstance(VSSettings vsSettings)
        {
            return new GeneralPage(vsSettings);
        }
    }
}
