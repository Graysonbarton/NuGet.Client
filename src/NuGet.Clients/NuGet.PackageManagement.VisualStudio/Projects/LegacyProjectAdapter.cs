// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#nullable enable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using NuGet.Commands.Restore;
using NuGet.ProjectManagement;
using NuGet.VisualStudio;

namespace NuGet.PackageManagement.VisualStudio.Projects;

internal class LegacyProjectAdapter : IProject
{
    private readonly IVsProjectAdapter _projectAdapter;

    public LegacyProjectAdapter(IVsProjectAdapter projectAdapter)
    {
        _projectAdapter = projectAdapter;
        OuterBuild = new TargetFrameworkAdapter(_projectAdapter);
        TargetFrameworks = new Dictionary<string, ITargetFramework>(StringComparer.OrdinalIgnoreCase)
        {
            [""] = OuterBuild
        };
    }

    public string FullPath => _projectAdapter.FullProjectPath;

    public string Directory => _projectAdapter.ProjectDirectory;

    public ITargetFramework OuterBuild { get; }

    public IReadOnlyDictionary<string, ITargetFramework> TargetFrameworks { get; }

    private class TargetFrameworkAdapter : ITargetFramework
    {
        private readonly IVsProjectAdapter _projectAdapter;

        public TargetFrameworkAdapter(IVsProjectAdapter projectAdapter)
        {
            _projectAdapter = projectAdapter;
        }

        public IReadOnlyList<IItem> GetItems(string itemType)
        {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();

            if (!ItemMetadataNames.TryGetValue(itemType, out var metadataNames))
            {
                throw new ArgumentException($"Unknown item type: {itemType}", nameof(itemType));
            }

            var items = _projectAdapter.GetBuildItemInformation(itemType, metadataNames);
            var result = new List<IItem>(items is ICollection collection ? collection.Count : 0);
            foreach (var (itemId, itemMetadata) in items)
            {
                var metadataDictionary = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                for (int i = 0; i < metadataNames.Length; i++)
                {
                    string? value = itemMetadata[i];
                    if (!string.IsNullOrEmpty(value))
                    {
                        metadataDictionary[metadataNames[i]] = itemMetadata[i];
                    }
                }

                var itemAdapter = new ItemAdapter
                {
                    Identity = itemId,
                    Metadata = metadataDictionary
                };
                result.Add(itemAdapter);
            }

            return result;
        }

        public string? GetProperty(string propertyName)
        {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
            string? value;
            if (FallbackProperties.Contains(propertyName))
            {
#pragma warning disable CS0618 // Type or member is obsolete
                value = _projectAdapter.BuildProperties.GetPropertyValueWithDteFallback(propertyName);
#pragma warning restore CS0618 // Type or member is obsolete
            }
            else
            {
                value = _projectAdapter.BuildProperties.GetPropertyValue(propertyName);
            }

            return string.IsNullOrEmpty(value) ? null : value;
        }

        // Do not add new properties here. New properties should only use new project system APIs.
        // In fact, ideally we should be migrating these properties
        private static readonly ImmutableHashSet<string> FallbackProperties = [
            ProjectBuildProperties.PackageId,
            ProjectBuildProperties.AssemblyName,
            ProjectBuildProperties.RestorePackagesPath,
            ProjectBuildProperties.RestoreSources,
            ProjectBuildProperties.RestoreAdditionalProjectSources,
            ProjectBuildProperties.RestoreFallbackFolders,
            ProjectBuildProperties.RestoreAdditionalProjectFallbackFolders,
            ProjectBuildProperties.PackageTargetFallback,
            ProjectBuildProperties.AssetTargetFallback,
            ProjectBuildProperties.ManagePackageVersionsCentrally,
            ProjectBuildProperties.RuntimeIdentifier,
            ProjectBuildProperties.RuntimeIdentifiers,
            ProjectBuildProperties.RuntimeSupports,
            ProjectBuildProperties.TreatWarningsAsErrors,
            ProjectBuildProperties.NoWarn,
            ProjectBuildProperties.WarningsAsErrors,
            ProjectBuildProperties.WarningsNotAsErrors,
            ProjectBuildProperties.RestorePackagesWithLockFile,
            ProjectBuildProperties.NuGetLockFilePath,
            ProjectBuildProperties.RestoreLockedMode,
            ProjectBuildProperties.CentralPackageVersionOverrideEnabled,
            ProjectBuildProperties.CentralPackageTransitivePinningEnabled,
            ProjectBuildProperties.MSBuildProjectExtensionsPath,
            ProjectBuildProperties.PackageVersion,
            ProjectBuildProperties.Version,
            ProjectBuildProperties.TargetPlatformIdentifier,
            ProjectBuildProperties.TargetPlatformVersion,
            ProjectBuildProperties.TargetPlatformMinVersion,
            ProjectBuildProperties.TargetFrameworkMoniker,
            ];
    }

    private record ItemAdapter : IItem
    {
        public required string Identity { get; init; }
        internal required IReadOnlyDictionary<string, string> Metadata { get; init; }

        public string? GetMetadata(string name)
        {
            if (Metadata.TryGetValue(name, out var value))
            {
                return value;
            }
            else
            {
                return null;
            }
        }
    }

    private static readonly string[] PackageReferenceMetadataNames =
        [
        "IsImplicitlyDefined",
        "Version",
        "VersionOverride",
        "GeneratePathProperty",
        "Aliases",
        "IncludeAssets",
        "ExcludeAssets",
        "PrivateAssets",
        "NoWarn",
        ];

    private static readonly string[] FrameworkReferenceMetadataNames = ["PrivateAssets"];

    private static readonly string[] VersionOnlyMetadataNames = ["Version"];

    private static readonly string[] ProjectReferenceMetadataNames =
        [
        "ReferenceOutputAssembly",
        "FullPath",
        "ExcludeAssets",
        "IncludeAssets",
        "PrivateAssets",
        ];

    internal static readonly IReadOnlyDictionary<string, string[]> ItemMetadataNames =
        new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
        {
            [ProjectItems.PackageReference] = PackageReferenceMetadataNames,
            [ProjectItems.PrunePackageReference] = VersionOnlyMetadataNames,
            [ProjectItems.PackageDownload] = VersionOnlyMetadataNames,
            [ProjectItems.FrameworkReference] = FrameworkReferenceMetadataNames,
            [ProjectItems.PackageVersion] = VersionOnlyMetadataNames,
            [ProjectItems.NuGetAuditSuppress] = [],
            [ProjectItems.ProjectReference] = ProjectReferenceMetadataNames,
        };
}
