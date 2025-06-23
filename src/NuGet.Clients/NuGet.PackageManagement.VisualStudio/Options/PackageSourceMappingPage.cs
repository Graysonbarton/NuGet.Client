// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Utilities.UnifiedSettings;
using NuGet.Configuration;
using NuGet.VisualStudio.Internal.Contracts;

namespace NuGet.PackageManagement.VisualStudio.Options
{
    [Guid("ACE317DA-8399-4DA4-9828-107E02244D45")]
    public class PackageSourceMappingPage : NuGetExternalSettingsProvider
    {
        internal const string MonikerPackageSourceMapping = "packageSourceMapping";
        internal const string MonikerSourceNameEnum = "packageSourceMapping.sourceName";
        internal const string MonikerPackageId = "packageId";
        internal const string MonikerSourceNames = "sourceName";

        private readonly IPackageSourceProvider _packageSourceProvider;
        private readonly PackageSourceMappingProvider _packageSourceMappingProvider;

        public PackageSourceMappingPage(VSSettings vsSettings, IPackageSourceProvider packageSourceProvider, PackageSourceMappingProvider packageSourceMappingProvider)
            : base(vsSettings)
        {
            _packageSourceProvider = packageSourceProvider ?? throw new ArgumentNullException(nameof(packageSourceProvider));
            _packageSourceMappingProvider = packageSourceMappingProvider ?? throw new ArgumentNullException(nameof(packageSourceMappingProvider));
        }

        public override async Task<ExternalSettingOperationResult<T>> GetValueAsync<T>(string moniker, CancellationToken cancellationToken)
        {
            if (moniker == MonikerPackageSourceMapping)
            {
                IReadOnlyList<PackageSourceMappingSourceItem> packageSourceMappingItems = await Task.Run(
                    () => _packageSourceMappingProvider.GetPackageSourceMappingItems(),
                    cancellationToken);

                Dictionary<string, List<PackageSourceContextInfo>> packageSourceMappingDictionary = CreatePackageSourceMappingDictionary(packageSourceMappingItems);
                ImmutableSortedDictionary<string, List<PackageSourceContextInfo>> sortedPackageSourceMappingDictionary
                    = packageSourceMappingDictionary.OrderBy(mapping => mapping.Key, StringComparer.OrdinalIgnoreCase).ToImmutableSortedDictionary();
                return GetValuePackageSourceMappings<T>(sortedPackageSourceMappingDictionary);
            }

            // Shouldn't happen as these are monikers we declared in registration.json.
            throw new InvalidOperationException();
        }

        public override Task<ExternalSettingOperationResult> SetValueAsync<T>(string moniker, T value, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public override async Task<ExternalSettingOperationResult<IReadOnlyList<EnumChoice>>> GetEnumChoicesAsync(string enumSettingMoniker, CancellationToken cancellationToken)
        {
            if (enumSettingMoniker == MonikerSourceNameEnum)
            {
                List<PackageSource> packageSources = await Task.Run(() => _packageSourceProvider.LoadPackageSources().ToList());

                // Source names should be unique, but in case validation was bypassed and multiple were added, de-duplicate them for the options shown for source mappings.
                List<string> sourceNames = packageSources
                    .Select(packageSource => packageSource.Name)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                List<EnumChoice> enumChoices = sourceNames.Select(sourceName => new EnumChoice(sourceName, sourceName)).ToList();
                return ExternalSettingOperationResult.SuccessResult((IReadOnlyList<EnumChoice>)enumChoices.AsReadOnly());
            }

            return await base.GetEnumChoicesAsync(enumSettingMoniker, cancellationToken);
        }

        private static ExternalSettingOperationResult<T> GetValuePackageSourceMappings<T>(ImmutableSortedDictionary<string, List<PackageSourceContextInfo>> packageSourceMappingsDictionary)
        {
            ExternalSettingOperationResult<T> result;

            try
            {
                var packageSourceMappingsList = new List<Dictionary<string, object>>(capacity: packageSourceMappingsDictionary.Count);

                // Each list item is represented by a dictionary, which in this case will have a single key-value pair for ConfigPath.
                foreach (KeyValuePair<string, List<PackageSourceContextInfo>> packageSourceMapping in packageSourceMappingsDictionary)
                {
                    string packageIdOrPattern = packageSourceMapping.Key;
                    List<PackageSourceContextInfo> packageSources = packageSourceMapping.Value;
                    IEnumerable<string> packageSourceNames = packageSources.Select(source => source.Name);
                    //string sourcesString = string.Join(", ", packageSourceNames);

                    var dict = new Dictionary<string, object>(capacity: 2)
                    {
                        { MonikerPackageId, packageIdOrPattern },
                        { MonikerSourceNames, packageSourceNames },
                    };

                    packageSourceMappingsList.Add(dict);
                }


                T castedPackageSources = (T)(object)packageSourceMappingsList;
                result = ExternalSettingOperationResult.SuccessResult(castedPackageSources);
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
            {
                var userErrorMessage = Strings.Error_NuGetConfig_InvalidState + " " + ex.Message;
                result = CreateSettingErrorResult<T>(userErrorMessage);

                var logErrorMessage = Strings.Error_NuGetConfig_InvalidState + " " + ex.ToString();
                ActivityLog.LogError(ExceptionHelper.LogEntrySource, logErrorMessage);
            }

            return result;
        }

        private static Dictionary<string, List<PackageSourceContextInfo>> CreatePackageSourceMappingDictionary(IReadOnlyList<PackageSourceMappingSourceItem> originalMappings)
        {
            var packageSourceMappingDictionary = new Dictionary<string, List<PackageSourceContextInfo>>();
            foreach (PackageSourceMappingSourceItem sourceItem in originalMappings)
            {
                foreach (PackagePatternItem patternItem in sourceItem.Patterns)
                {
                    if (!packageSourceMappingDictionary.ContainsKey(patternItem.Pattern))
                    {
                        packageSourceMappingDictionary[patternItem.Pattern] = new List<PackageSourceContextInfo>();
                    }
                    packageSourceMappingDictionary[patternItem.Pattern].Add(new PackageSourceContextInfo(sourceItem.Key));
                }
            }

            return packageSourceMappingDictionary;
        }
    }
}
