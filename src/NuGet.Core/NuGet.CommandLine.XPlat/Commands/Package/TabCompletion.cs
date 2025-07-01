// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NuGet.Common;
using NuGet.Configuration;
using NuGet.Credentials;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;

namespace NuGet.CommandLine.XPlat.Commands.Package;

internal static class TabCompletion
{
    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(1);

    public static void Setup()
    {
        // for tab completion, we can't output anything to the console, which also means non-interactive mode only.
        DefaultCredentialServiceUtility.SetupDefaultCredentialService(NullLogger.Instance, nonInteractive: true);
        XPlatUtility.ConfigureProtocol();
    }

    public static async Task<IEnumerable<string>> GetPackageCompletionsAsync(string idStem, bool allowPrerelease, string configDirectory)
    {
        using var timeout = new CancellationTokenSource(DefaultTimeout);

        ISettings settings = Settings.LoadDefaultSettings(configDirectory);
        var packageSources = SettingsUtility.GetEnabledSources(settings);

        var packageIdLists = await Task.WhenAll(packageSources.Select(async source =>
        {
            var sourceRepository = Repository.Factory.GetCoreV3(source);
            var autoCompleteResource = await sourceRepository.GetResourceAsync<AutoCompleteResource>(timeout.Token);
            if (autoCompleteResource is null) return [];
            var packageIds = await autoCompleteResource.IdStartsWith(idStem, allowPrerelease, NullLogger.Instance, timeout.Token);
            return packageIds;
        }));

        return packageIdLists
            .SelectMany(packageIds => packageIds)
            .Distinct()
            .OrderByDescending(id => id, StringComparer.OrdinalIgnoreCase);
    }

    public static async Task<IEnumerable<NuGetVersion>> GetVersionsAsync(string packageId, string versionFragment, bool allowPrerelease, string configDirectory)
    {
        using var timeout = new CancellationTokenSource(DefaultTimeout);

        ISettings settings = Settings.LoadDefaultSettings(configDirectory);
        var packageSources = SettingsUtility.GetEnabledSources(settings);
        var packageSourceMapping = PackageSourceMapping.GetPackageSourceMapping(settings);
        HashSet<string>? configuredSources = packageSourceMapping.IsEnabled ? packageSourceMapping.GetConfiguredPackageSources(packageId).ToHashSet() : null;
        var cacheContext = new SourceCacheContext
        {
            IgnoreFailedSources = true,
        };

        var versionsLists = await Task.WhenAll(packageSources.Select(async source =>
        {
            if (configuredSources is not null && !configuredSources.Contains(source.Name))
            {
                return [];
            }

            var sourceRepository = Repository.Factory.GetCoreV3(source);
            var autoCompleteResource = await sourceRepository.GetResourceAsync<AutoCompleteResource>(timeout.Token);
            if (autoCompleteResource is null) return [];

            var versions = await autoCompleteResource.VersionStartsWith(
                packageId,
                versionFragment,
                allowPrerelease,
                cacheContext,
                NullLogger.Instance,
                timeout.Token);
            return versions;
        }));

        return versionsLists
            .SelectMany(versions => versions)
            .Distinct()
            .OrderDescending();
    }
}
