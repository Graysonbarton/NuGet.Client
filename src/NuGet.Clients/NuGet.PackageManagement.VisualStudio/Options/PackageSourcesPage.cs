// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Utilities.UnifiedSettings;
using NuGet.Configuration;

namespace NuGet.PackageManagement.VisualStudio.Options
{
    [Guid("15C605EC-4FD7-446B-BA4A-75ECF0C0B2D0")]
    public class PackageSourcesPage : NuGetExternalSettingsProvider
    {
        internal const string MonikerPackageSources = "packageSources";
        internal const string MonikerMachineWideSources = "machineWidePackageSources";
        internal const string MonikerSourceName = "sourceName";
        internal const string MonikerSourceUrl = "sourceUrl";
        internal const string MonikerIsEnabled = "isEnabled";
        private IPackageSourceProvider _packageSourceProvider;
        internal List<PackageSource> _packageSources;
        internal List<PackageSource> _machineWidePackageSources;

        public PackageSourcesPage(VSSettings vsSettings, IPackageSourceProvider packageSourceProvider)
            : base(vsSettings)
        {
            _packageSourceProvider = packageSourceProvider ?? throw new ArgumentNullException(nameof(packageSourceProvider));
            LoadPackageSources();
        }

        private void LoadPackageSources()
        {
            IEnumerable<PackageSource> all = _packageSourceProvider.LoadPackageSources();
            _packageSources = all.Where(packageSource => packageSource.IsMachineWide == false).ToList();
            _machineWidePackageSources = all.Where(packageSource => packageSource.IsMachineWide == true).ToList();
        }

        /// <summary>
        /// Reset any cached values for this specific page instance when the settings change.
        /// </summary>
        internal override void VsSettings_SettingsChanged(object sender, EventArgs e)
        {
            LoadPackageSources();
            base.VsSettings_SettingsChanged(sender, e);
        }

        public override Task<ExternalSettingOperationResult<T>> GetValueAsync<T>(string moniker, CancellationToken cancellationToken)
        {
            switch (moniker)
            {
                case MonikerPackageSources: return LoadPackageSourcesSetting<T>(_packageSources);
                case MonikerMachineWideSources: return LoadPackageSourcesSetting<T>(_machineWidePackageSources);
                default: break;
            }

            // Shouldn't happen as these are monikers we declared in registration.json.
            throw new InvalidOperationException();
        }

        public override Task<ExternalSettingOperationResult> SetValueAsync<T>(string moniker, T value, CancellationToken cancellationToken)
        {
            var packageSourcesList = value as IList<IDictionary<string, object>>;
            if (packageSourcesList is null)
            {
                // Shouldn't happen as these are monikers we declared in registration.json.
                throw new InvalidOperationException();
            }

            switch (moniker)
            {
                case MonikerPackageSources:
                    {
                        return SetValuePackageSources<T>(packageSourcesList);
                    }
                case MonikerMachineWideSources:
                    {
                        return SetEnabledOnPackageSource(packageSourcesList);
                    }
                default: break;
            }

            // Shouldn't happen as these are monikers we declared in registration.json.
            throw new InvalidOperationException();
        }

        private Task<ExternalSettingOperationResult> SetEnabledOnPackageSource(IList<IDictionary<string, object>> packageSourcesList)
        {
            ExternalSettingOperationResult result;

            try
            {
                foreach (PackageSource originalMachineWideSource in _machineWidePackageSources)
                {
                    string originalPackageSourceName = originalMachineWideSource.Name;
                    IDictionary<string, object> targetPackageSource = packageSourcesList
                        .Single(packageSourceDictionary =>
                            packageSourceDictionary[MonikerSourceName].ToString() == originalPackageSourceName);

                    bool originalIsEnabled = originalMachineWideSource.IsEnabled;
                    bool targetIsEnabled = (bool)targetPackageSource[MonikerIsEnabled];

                    if (originalIsEnabled != targetIsEnabled)
                    {
                        if (targetIsEnabled)
                        {
                            _packageSourceProvider.EnablePackageSource(originalPackageSourceName);
                        }
                        else
                        {
                            _packageSourceProvider.DisablePackageSource(originalPackageSourceName);
                        }
                    }

                    // Only one value can be set at a time from Unified Settings, so we're done once a change is made.
                    break;
                }

                result = ExternalSettingOperationResult.Success.Instance;
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception ex)
            {
                result = CreateSettingErrorResult(ex.Message + " ('" + MonikerMachineWideSources + "')");
            }
#pragma warning restore CA1031 // Do not catch general exception types

            return Task.FromResult(result);
        }

        private Task<ExternalSettingOperationResult> SetValuePackageSources<T>(IList<IDictionary<string, object>> packageSourcesList)
        {
            ExternalSettingOperationResult result;

            try
            {
                List<PackageSource> packageSources = new List<PackageSource>(capacity: packageSourcesList.Count);

                foreach (Dictionary<string, object> packageSourceDictionary in packageSourcesList)
                {
                    string name = packageSourceDictionary[MonikerSourceName].ToString();
                    string source = packageSourceDictionary[MonikerSourceUrl].ToString();
                    bool isEnabled = (bool)packageSourceDictionary[MonikerIsEnabled];

                    PackageSource packageSource = new PackageSource(source, name, isEnabled);

                    if (packageSource.IsHttp && !packageSource.IsHttps)
                    {
                        packageSource.AllowInsecureConnections = true;
                    }
                    packageSources.Add(packageSource);
                }

                _packageSourceProvider.SavePackageSources(packageSources);
                result = ExternalSettingOperationResult.Success.Instance;
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception ex)
            {
                result = CreateSettingErrorResult(Strings.Error_ApplySetting_Failed + " " + ex.Message);
                ActivityLog.LogError(ExceptionHelper.LogEntrySource, ex.ToString());
            }
#pragma warning restore CA1031 // Do not catch general exception types

            return Task.FromResult(result);
        }

        private static Task<ExternalSettingOperationResult<T>> LoadPackageSourcesSetting<T>(List<PackageSource> packageSources)
        {
            ExternalSettingOperationResult<T> result;

            try
            {
                var packageSourcesList = new List<Dictionary<string, object>>(capacity: packageSources.Count);

                // Each list item is represented by a dictionary, which in this case will have a single key-value pair for ConfigPath.
                foreach (PackageSource packageSource in packageSources)
                {
                    var dict = new Dictionary<string, object>(capacity: 3)
                    {
                        { MonikerSourceName, packageSource.Name },
                        { MonikerSourceUrl, packageSource.SourceUri },
                        { MonikerIsEnabled, packageSource.IsEnabled },
                    };

                    packageSourcesList.Add(dict);
                }

                var castedPackageSources = (T)(object)packageSourcesList;
                result = ExternalSettingOperationResult.SuccessResult(castedPackageSources);
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception ex)
            {
                var errorMessage = string.Format(CultureInfo.CurrentCulture, Strings.Error_NuGetConfig_InvalidState, ex.Message);
                result = CreateSettingErrorResult<T>(errorMessage);
            }
#pragma warning restore CA1031 // Do not catch general exception types

            return Task.FromResult(result);
        }
    }
}
