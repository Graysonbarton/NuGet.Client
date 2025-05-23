// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.Utilities.UnifiedSettings;
using NuGet.Configuration;
using NuGet.PackageManagement.VisualStudio.Options;
using Test.Utility;
using Xunit;

namespace NuGet.PackageManagement.VisualStudio.Test.Options
{
    public class PackageSourcesPageTests : NuGetExternalSettingsProviderTests<PackageSourcesPage>
    {
        protected override PackageSourcesPage CreateInstance(VSSettings vsSettings)
        {
            TestPackageSourceProvider packageSourceProvider = new(
                packageSources: Enumerable.Empty<PackageSource>());

            return new PackageSourcesPage(vsSettings, packageSourceProvider);
        }

        [Fact]
        public async Task SetValueAsync_WithIncompleteUri_ReturnsFailureResultTaskAsync()
        {
            // Arrange
            PackageSourcesPage instance = CreateInstance(_vsSettings);
            string inputIncompleteUri = "https://";
            Dictionary<string, object> packageSourceDictionary = new Dictionary<string, object>();

            packageSourceDictionary["sourceName"] = "unitTestingSourceName";
            packageSourceDictionary["sourceUrl"] = inputIncompleteUri;
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
            result.Should()
                .BeEquivalentTo(ExternalSettingOperationResult.Failure.FailureResultTask<List<Dictionary<string, object>>>);
        }
    }
}
