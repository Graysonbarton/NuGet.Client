// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Utilities.UnifiedSettings;
using NuGet.Configuration;

namespace NuGet.PackageManagement.VisualStudio.Options
{
    [Guid("15C605EC-4FD7-446B-BA4A-75ECF0C0B2D0")]
    public class PackageSourcesPage : NuGetExternalSettingsProvider
    {
        private const string MonikerPackageSources = "packageSources";
        private IPackageSourceProvider _packageSourceProvider;

        public PackageSourcesPage(VSSettings vsSettings, IPackageSourceProvider packageSourceProvider)
            : base(vsSettings)
        {
            _packageSourceProvider = packageSourceProvider ?? throw new ArgumentNullException(nameof(packageSourceProvider));
        }

        public override Task<ExternalSettingOperationResult<T>> GetValueAsync<T>(string moniker, CancellationToken cancellationToken)
        {
            switch (moniker)
            {
                case MonikerPackageSources: return LoadPackageSourcesOrThrow<T>();
                default: break;
            }

            // Shouldn't happen as these are monikers we declared in registration.json.
            throw new InvalidOperationException();
        }

        public override Task<ExternalSettingOperationResult> SetValueAsync<T>(string moniker, T value, CancellationToken cancellationToken)
        {
            var packageSourcesList = value as IList<IDictionary<string, object>>;
            if (moniker != MonikerPackageSources || packageSourcesList is null)
            {
                // Shouldn't happen as these are monikers we declared in registration.json.
                throw new InvalidOperationException();
            }

            List<PackageSource> packageSources = new List<PackageSource>(capacity: packageSourcesList.Count);

            foreach (Dictionary<string, object> packageSourceDictionary in packageSourcesList)
            {
                string name = packageSourceDictionary["sourceName"].ToString();
                string source = packageSourceDictionary["sourceUrl"].ToString();
                bool isEnabled = (bool)packageSourceDictionary["isEnabled"];

                PackageSource packageSource = new PackageSource(source, name, isEnabled, isOfficial: false);
                if (packageSource.IsHttp && !packageSource.IsHttps)
                {
                    packageSource.AllowInsecureConnections = true;
                }
                packageSources.Add(packageSource);
            }

            _packageSourceProvider.SavePackageSources(packageSources);
            return Task.FromResult((ExternalSettingOperationResult)ExternalSettingOperationResult.Success.Instance);
        }

        private Task<ExternalSettingOperationResult<T>> LoadPackageSourcesOrThrow<T>()
        {
            ExternalSettingOperationResult<T> result;
#pragma warning disable CA1031 // Do not catch general exception types
            try
            {
                List<PackageSource> packageSources = _packageSourceProvider.LoadPackageSources().ToList();

                var packageSourcesList = new List<Dictionary<string, object>>(capacity: packageSources.Count);

                // Each list item is represented by a dictionary, which in this case will have a single key-value pair for ConfigPath.
                foreach (var packageSource in packageSources)
                {
                    var dict = new Dictionary<string, object>(capacity: 3)
                    {
                        { "isEnabled", packageSource.IsEnabled },
                        { "sourceName", packageSource.Name },
                        { "sourceUrl", packageSource.SourceUri },
                    };

                    packageSourcesList.Add(dict);
                }

                var castedConfigPaths = (T)(object)packageSourcesList;
                result = ExternalSettingOperationResult.SuccessResult(castedConfigPaths);
            }
            catch (Exception ex)
            {
                result = CreateSettingErrorResult<T>(ex.Message + " ('" + MonikerPackageSources + "')");
            }
#pragma warning restore CA1031 // Do not catch general exception types

            return Task.FromResult(result);
        }
    }
}
