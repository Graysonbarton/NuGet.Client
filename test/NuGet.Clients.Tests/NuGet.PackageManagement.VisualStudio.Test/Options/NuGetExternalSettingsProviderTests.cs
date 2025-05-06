// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#nullable enable

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NuGet.PackageManagement.VisualStudio.Options;
using Xunit;

namespace NuGet.PackageManagement.VisualStudio.Test.Options
{
    public abstract class NuGetExternalSettingsProviderTests<T> where T : NuGetExternalSettingsProvider
    {
        private VSSettings _vsSettings;

        protected abstract T CreateInstance(VSSettings? vsSettings);
        protected abstract List<string> GetMonikers();

        protected NuGetExternalSettingsProviderTests()
        {
            _vsSettings = Mock.Of<VSSettings>();
        }

        [Fact]
        public virtual async Task GetValueAsync_MonikerExists_ReturnsSuccessExternalSettingOperationResultAsync()
        {
            // Arrange
            T page = CreateInstance(_vsSettings);
            List<string> monikers = GetMonikers();
            monikers.Should().NotBeNullOrEmpty();

            // Act
            Func<Task> act = async () =>
            {
                foreach (string moniker in monikers)
                {
                    var result = await page.GetValueAsync<object>(moniker, CancellationToken.None);
                    result.Should().NotBeNull();
                }
            };

            // Assert
            await act.Invoke().Should().NotThrow(because: "All GetValue calls were for existing monikers");
        }

        public virtual Task GetValueAsync_MonikerDoesNotExist_ThrowsInvalidOperationExceptionAsync()
        {

        }

        public virtual Task GetValueAsync_ValueInvalidForMoniker_ThrowsIncompatibleSettingTypeExceptionAsync()
        {

        }

        public virtual Task SetValueAsync_MonikerExists_ReturnsSuccessExternalSettingOperationResultAsync()
        {

        }

        public virtual Task SetValueAsync_MonikerDoesNotExist_InvalidOperationExceptionAsync()
        {

        }

        public virtual Task SetValueAsync_ValueInvalidForMoniker_ThrowsIncompatibleSettingTypeExceptionAsync()
        {

        }

        [Fact]
        public void Constructor_NullVsSettings_ThrowsArgumentNullException()
        {
            // Arrange
            Mock<NuGetExternalSettingsProvider> nuGetExternalSettingsProvider = new Mock<NuGetExternalSettingsProvider>();

            // Act
            Action act = () => CreateInstance(vsSettings: null);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }
    }
}
