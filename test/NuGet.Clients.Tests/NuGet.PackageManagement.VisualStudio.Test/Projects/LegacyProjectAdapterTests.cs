// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using NuGet.Commands.Restore;
using NuGet.Commands.Restore.Utility;
using NuGet.Configuration;
using NuGet.PackageManagement.VisualStudio.Projects;
using NuGet.ProjectManagement;
using Xunit;

namespace NuGet.PackageManagement.VisualStudio.Test.Projects;

public class LegacyProjectAdapterTests
{
    [Fact]
    public void ItemMetadataNames_ContainsAllMetadataRequired()
    {
        // Arrange
        var projectProperties = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "MSBuildProjectName", "Contoso.Project" },
            { ProjectBuildProperties.TargetFrameworkIdentifier, ".NETFramework" },
            { ProjectBuildProperties.TargetFrameworkVersion, "v4.7.2" },
            { ProjectBuildProperties.TargetFrameworkMoniker, ".NETFramework,Version=v4.7.2" },
        };

        var packageReference = new TestItem
        {
            Identity = "Contoso.Utils",
            Metadata = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        };
        var packageVersion = new TestItem
        {
            Identity = "Contoso.Utils",
            Metadata = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "Version", "1.0.0" }
            }
        };
        var projectReference = new TestItem
        {
            Identity = "Contoso.Project",
            Metadata = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "FullPath", @"C:\Projects\Contoso\Contoso.Project.csproj" },
                { "ReferenceOutputAssembly", "true" }
            }
        };
        var frameworkReference = new TestItem
        {
            Identity = "Microsoft.AspNetCore.App",
            Metadata = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        };

        var nugetAuditSuppress = new TestItem
        {
            Identity = "https://cve.contoso.test/1",
            Metadata = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        };

        var packageDownload = new TestItem
        {
            Identity = "Contoso.BuildTools",
            Metadata = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "Version", "[2.0.0]" },
            }
        };

        Dictionary<string, IReadOnlyList<IItem>> items = new Dictionary<string, IReadOnlyList<IItem>>(StringComparer.OrdinalIgnoreCase)
        {
            { "PackageReference", new[] { packageReference } },
            { "PackageVersion", new[] { packageVersion } },
            { "ProjectReference", new[] { projectReference } },
            { "FrameworkReference", new[] { frameworkReference } },
            { "NuGetAuditSuppress", new[] { nugetAuditSuppress } },
            { "PackageDownload", new[] { packageDownload } },
        };

        var projectEvaluation = new TestTargetFramework
        {
            Properties = projectProperties,
            Items = items
        };

        var project = new TestProjectAdapter
        {
            FullPath = @"C:\Projects\Contoso\Contoso.csproj",
            Directory = @"C:\Projects\Contoso",
            OuterBuild = projectEvaluation,
            TargetFrameworks = new Dictionary<string, ITargetFramework>(StringComparer.OrdinalIgnoreCase)
            {
                { "", projectEvaluation }
            }
        };

        // Act
        _ = PackageSpecFactory.GetPackageSpec(project, NullSettings.Instance);

        // Assert
    }

    private record TestItem : IItem
    {
        public required string Identity { get; init; }
        public required IReadOnlyDictionary<string, string> Metadata { get; init; }

        public string? GetMetadata(string name)
        {
            if (Metadata.TryGetValue(name, out var value))
            {
                return value;
            }
            return null;
        }
    }

    private record TestProjectAdapter : IProject
    {
        public required string FullPath { get; init; }
        public required string Directory { get; init; }
        public required ITargetFramework OuterBuild { get; init; }
        public required IReadOnlyDictionary<string, ITargetFramework> TargetFrameworks { get; init; }
    }

    private record TestTargetFramework : ITargetFramework
    {
        public required IReadOnlyDictionary<string, string> Properties { get; init; }
        public required IReadOnlyDictionary<string, IReadOnlyList<IItem>> Items { get; init; }

        public string? GetProperty(string propertyName)
        {
            return Properties.TryGetValue(propertyName, out var value) ? value : null;
        }

        public IReadOnlyList<IItem> GetItems(string itemType)
        {
            if (!LegacyProjectAdapter.ItemMetadataNames.TryGetValue(itemType, out var metadataNames))
            {
                throw new Exception($"{nameof(LegacyProjectAdapter)}.{nameof(LegacyProjectAdapter.ItemMetadataNames)} is missing metadata names for {itemType}");
            }

            if (!Items.TryGetValue(itemType, out var items))
            {
                throw new Exception("Test is missing items for type: " + itemType);
            }

            var result = items
                .Select(item => new ValidatingItem
                {
                    ItemType = itemType,
                    InnerItem = item,
                    KnownMetadataNames = metadataNames
                }).ToList();
            return result;
        }
    }

    private record ValidatingItem : IItem
    {
        public required string ItemType { get; init; }
        public required IItem InnerItem { get; init; }
        public required IReadOnlyList<string> KnownMetadataNames { get; init; }

        public string Identity => InnerItem.Identity;

        public string? GetMetadata(string name)
        {
            if (!KnownMetadataNames.Contains(name, StringComparer.OrdinalIgnoreCase))
            {
                throw new ArgumentException($"Unknown metadata name '{name}' on type '{ItemType}'", nameof(name));
            }

            string? value = InnerItem.GetMetadata(name);
            return value;
        }
    }
}
