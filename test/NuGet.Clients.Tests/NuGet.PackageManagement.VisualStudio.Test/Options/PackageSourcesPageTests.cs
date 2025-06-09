// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.Sdk.TestFramework;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Utilities.UnifiedSettings;
using NuGet.Configuration;
using NuGet.PackageManagement.VisualStudio.Options;
using NuGet.VisualStudio;
using Test.Utility;
using Xunit;

namespace NuGet.PackageManagement.VisualStudio.Test.Options
{
    [Collection(MockedVS.Collection)]
    public class PackageSourcesPageTests : NuGetExternalSettingsProviderTests<PackageSourcesPage>
    {
        public PackageSourcesPageTests(GlobalServiceProvider sp)
        {
            sp.Reset();
            NuGetUIThreadHelper.SetCustomJoinableTaskFactory(ThreadHelper.JoinableTaskFactory);
        }

        protected override PackageSourcesPage CreateInstance(VSSettings vsSettings)
        {
            TestPackageSourceProvider packageSourceProvider = new(
                packageSources: Enumerable.Empty<PackageSource>());

            return new PackageSourcesPage(vsSettings, packageSourceProvider);
        }

        [Theory]
        [InlineData(@"http://")]
        [InlineData(@"https://")]
        [InlineData(@"https:// ")]
        [InlineData(@" https://")]
        [InlineData(@"ftp://")]
        [InlineData(@"http:/")]
        public async Task SetValueAsync_WithInvalidRemoteSource_ReturnsFailureResultTaskAsync(string invalidSource)
        {
            // Arrange
            PackageSourcesPage instance = CreateInstance(_vsSettings);
            Dictionary<string, object> packageSourceDictionary = new Dictionary<string, object>();

            packageSourceDictionary["sourceName"] = "unitTestingSourceName";
            packageSourceDictionary["sourceUrl"] = invalidSource;
            packageSourceDictionary["isEnabled"] = true;

            IList<IDictionary<string, object>> packageSourceDictionaryList =
                new List<IDictionary<string, object>>(capacity: 1)
                {
                    packageSourceDictionary
                };

            // Act
            ExternalSettingOperationResult result = await instance.SetValueAsync(
                PackageSourcesPage.MonikerPackageSources,
                packageSourceDictionaryList,
                CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<ExternalSettingOperationResult.Failure>();

            var failure = result as ExternalSettingOperationResult.Failure;
            failure.IsTransient.Should().BeTrue();
            failure.Scope.Should().Be(ExternalSettingsErrorScope.SingleSettingOnly);
            failure.ErrorMessage.Should().StartWith(Strings.Error_PackageSource_InvalidSource);
        }

        [Theory]
        [InlineData(@"C")]
        [InlineData(@"http")] // Missing :// causes this to be treated as a file path.
        [InlineData(@"http:")]
        [InlineData(@"ftp")] // Missing :// causes this to be treated as a file path.
        [InlineData(@"C:")]
        [InlineData(@"C:\invalid\*\'\chars")]
        [InlineData(@"\\server\invalid\*\")]
        [InlineData(@"..\packages")]
        [InlineData(@"./configs/source.config")]
        [InlineData(@"../local-packages/")]
        public async Task SetValueAsync_WithInvalidUncPath_ReturnsFailureResultTaskAsync(string invalidSource)
        {
            // Arrange
            PackageSourcesPage instance = CreateInstance(_vsSettings);
            Dictionary<string, object> packageSourceDictionary = new Dictionary<string, object>();

            packageSourceDictionary["sourceName"] = "unitTestingSourceName";
            packageSourceDictionary["sourceUrl"] = invalidSource;
            packageSourceDictionary["isEnabled"] = true;

            IList<IDictionary<string, object>> packageSourceDictionaryList =
                new List<IDictionary<string, object>>(capacity: 1)
                {
                    packageSourceDictionary
                };

            // Act
            ExternalSettingOperationResult result = await instance.SetValueAsync(
                PackageSourcesPage.MonikerPackageSources,
                packageSourceDictionaryList,
                CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<ExternalSettingOperationResult.Failure>();

            var failure = result as ExternalSettingOperationResult.Failure;
            failure.IsTransient.Should().BeTrue();
            failure.Scope.Should().Be(ExternalSettingsErrorScope.SingleSettingOnly);
            failure.ErrorMessage.Should().StartWith(Strings.Error_PackageSource_InvalidSource);
        }
    }
}
