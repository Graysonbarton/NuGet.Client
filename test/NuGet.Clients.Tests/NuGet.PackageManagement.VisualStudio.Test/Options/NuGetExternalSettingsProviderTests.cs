// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#nullable enable

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NuGet.Configuration;
using NuGet.PackageManagement.VisualStudio.Options;
using NuGet.PackageManagement.VisualStudio.Utility.FileWatchers;
using Xunit;

namespace NuGet.PackageManagement.VisualStudio.Test.Options
{
    public abstract class NuGetExternalSettingsProviderTests<TPage> where TPage : NuGetExternalSettingsProvider
    {
        private VSSettings _vsSettings;

        protected abstract TPage CreateInstance(VSSettings? vsSettings);
        protected abstract List<string> GetMonikers();

        protected NuGetExternalSettingsProviderTests()
        {
            var solutionManager = new Mock<ISolutionManager>();
            var machineWideSettings = new Mock<IMachineWideSettings>();
            var slnConfigWatcher = new Mock<IFileWatcher>();
            var userConfigWatcher = new Mock<IFileWatcher>();

            var watcherFactory = new Mock<IFileWatcherFactory>();
            watcherFactory.Setup(f => f.CreateUserConfigFileWatcher()).Returns(userConfigWatcher.Object);
            watcherFactory.Setup(f => f.CreateSolutionConfigFileWatcher(It.IsAny<string>())).Returns(userConfigWatcher.Object);

            _vsSettings = new VSSettings(solutionManager.Object, machineWideSettings.Object, watcherFactory.Object);
        }

        //public abstract Task GetValueAsync_MonikerExists_ReturnsSuccessExternalSettingOperationResultAsync();

        [Fact]
        public virtual async Task GetValueAsync_MonikerDoesNotExist_ThrowsInvalidOperationExceptionAsync()
        {
            // Arrange
            VSSettings vsSettings = _vsSettings;
            TPage instance = CreateInstance(vsSettings);

            // Act
            Func<Task> act = () => instance.GetValueAsync<object>(moniker: "TESTING!doesNotExist", CancellationToken.None);

            // Assert
            await Assert.ThrowsAsync<InvalidOperationException>(act);
        }

        [Fact]
        public void Constructor_NullVsSettings_ThrowsArgumentNullException()
        {
            // Arrange
            VSSettings? vsSettings = null;

            // Act
            Action act = () => CreateInstance(vsSettings);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }
    }
}
