// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.Sdk.TestFramework;
using NuGet.PackageManagement.VisualStudio.Services;
using NuGet.Test.Utility;
using Xunit;

namespace NuGet.PackageManagement.VisualStudio.Test.Services
{
    [Collection(MockedVS.Collection)]
    public class OpenFileServiceTests : MockedVSCollectionTests, IClassFixture<TextFileFixture>
    {
        private readonly OpenFileService _service;
        private readonly string _validFilePath;

        //Write tests for the OpenFileService class
        public OpenFileServiceTests(GlobalServiceProvider globalServiceProvider, TextFileFixture textFileFixture)
            : base(globalServiceProvider)
        {
            globalServiceProvider.Reset();
            _service = new OpenFileService();
            _validFilePath = textFileFixture.FullPath;
        }

        [Fact]
        public async Task IsEnabledAsync_WhenFilePathProvided_ShouldBeTrueAsync()
        {
            // Arrange
            Dictionary<string, object> dictionaryFilePaths = new Dictionary<string, object>
            {
                { OpenFileService.FILE_PATH, _validFilePath }
            };

            // Act
            var result = await _service.IsEnabledAsync(dictionaryFilePaths, CancellationToken.None);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task IsEnabledAsync_WhenMissingFilePathKey_ShouldBeFalseAsync()
        {
            // Arrange
            Dictionary<string, object> dictionaryFilePaths = new Dictionary<string, object>
            {
                { "invalidKey", _validFilePath }
            };

            // Act
            var result = await _service.IsEnabledAsync(dictionaryFilePaths, CancellationToken.None);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task IsEnabledAsync_WhenFilePathDoesNotExist_ShouldBeFalseAsync()
        {
            // Arrange
            string invalidPath = "pathDoesNotExist/NuGet.Config";
            Dictionary<string, object> dictionaryFilePaths = new Dictionary<string, object>
            {
                { OpenFileService.FILE_PATH, invalidPath }
            };

            // Act
            var result = await _service.IsEnabledAsync(dictionaryFilePaths, CancellationToken.None);

            // Assert
            result.Should().BeFalse();
        }
    }

    public class TextFileFixture : IDisposable
    {
        private TestDirectory _testDirectory;

        public string Directory { get; init; }
        public string FullPath { get; init; }
        public string FileContents { get; init; }

        public TextFileFixture()
        {
            _testDirectory = TestDirectory.Create();
            Directory = _testDirectory.Path;
            FullPath = Path.Combine(Directory, "NuGet.Config");
            FileContents = "Test contents";
            File.WriteAllText(FullPath, contents: FileContents);
        }
        public void Dispose()
        {
            _testDirectory.Dispose(); // Ensure the test directory is cleaned up
        }
    }
}
