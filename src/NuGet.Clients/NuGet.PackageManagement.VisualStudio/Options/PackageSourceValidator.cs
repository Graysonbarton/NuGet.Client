// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using NuGet.Configuration;

namespace NuGet.PackageManagement.VisualStudio.Options
{
    internal static class PackageSourceValidator
    {
        internal static PackageSource FindExistingOrCreate(
            string source,
            string name,
            bool isEnabled,
            List<PackageSource> packageSources)
        {
            string trimmedSource = source?.Trim() ?? string.Empty;
            string trimmedName = name?.Trim() ?? string.Empty;

            PackageSource? foundByName = FindByName(trimmedName, packageSources);
            if (foundByName is not null)
            {
                return foundByName;
            }

            PackageSource? foundBySource = FindBySource(trimmedSource, packageSources);
            if (foundBySource is not null)
            {
                return foundBySource;
            }

            // Create and validate a new Package Source since none was found by name or source.
            var packageSource = new PackageSource(trimmedSource, trimmedName, isEnabled);
            SetAllowInsecureConnectionsProperty(packageSource);
            EnsureValidSources(packageSource);

            return packageSource;
        }

        /// <summary>
        /// Validates the Uri of a remote or local package source.
        /// The regex used here matches the the error message declared in the Unified Settings registration.json file
        /// for the package sources page.
        /// </summary>
        /// <param name="packageSource"></param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        internal static void EnsureValidSources(PackageSource packageSource)
        {
            _ = packageSource ?? throw new ArgumentNullException(nameof(packageSource));
            string source = packageSource.Source;

            if (packageSource.IsHttp)
            {
                // This check is copied from the registration.json and should be kept in sync.See section:
                // 'nuGetPackageManager.packageSources.externalSettings -> properties -> packageSources -> properties -> sourceUrl'.
                if (!Regex.IsMatch(
                    input: source,
                    pattern: "^(?:https?://[^\\s]+)"))
                {
                    throw new ArgumentOutOfRangeException(
                        paramName: nameof(PackageSource.Source),
                        actualValue: source,
                        message: Strings.Error_PackageSourceUriProtocol_NotSupported);
                }
            }
            else if (!Common.PathValidator.IsValidLocalPath(source) &&
                !Common.PathValidator.IsValidUncPath(source) &&
                !Common.PathValidator.IsValidUrl(source))
            {
                // This check is copied from the registration.json and should be kept in sync.See section:
                // 'nuGetPackageManager.packageSources.externalSettings -> properties -> packageSources -> properties -> sourceUrl'.
                //Regex.Match(
                //    input: source,
                //    pattern: "^(?:https?://[^\\s]+)");
                //| [a - zA - Z]\\\\:\\\\\\\\[^\\\\s] *
                throw new ArgumentOutOfRangeException(
                    paramName: nameof(PackageSource.Source),
                    actualValue: source,
                    Strings.Error_PackageSource_InvalidSource);
            }
        }

        internal static void ValidateUniquenessOrThrow(List<PackageSource> packageSources)
        {
            _ = packageSources ?? throw new ArgumentNullException(nameof(packageSources));

            EnsureUniqueNames(packageSources);
            EnsureUniqueSources(packageSources);
        }

        private static void EnsureUniqueNames(List<PackageSource> packageSources)
        {
            var seen = new HashSet<string>(
                capacity: packageSources.Count,
                comparer: StringComparer.CurrentCultureIgnoreCase);

            foreach (PackageSource packageSource in packageSources)
            {
                if (!seen.Add(packageSource.Name.Trim()))
                {
                    throw new ArgumentException(message: Strings.Error_PackageSource_UniqueName);
                }
            }
        }

        private static void EnsureUniqueSources(List<PackageSource> packageSources)
        {
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (PackageSource packageSource in packageSources)
            {
                string trimmedSource = packageSource.Source?.Trim() ?? string.Empty;

                bool isDuplicate;
                if (packageSource.IsLocal)
                {
                    string canonicalPath = PathValidator.GetCanonicalPath(trimmedSource);
                    isDuplicate = !seen.Add(canonicalPath);
                }
                else
                {
                    isDuplicate = !seen.Add(trimmedSource);
                }

                if (isDuplicate)
                {
                    throw new ArgumentException(message: Strings.Error_PackageSource_UniqueSource);
                }
            }
        }

        private static void SetAllowInsecureConnectionsProperty(PackageSource packageSource)
        {
            _ = packageSource ?? throw new ArgumentNullException(nameof(packageSource));

            if (packageSource.IsHttp && !packageSource.IsHttps)
            {
                packageSource.AllowInsecureConnections = true;
            }
        }

        private static PackageSource? FindByName(string name, List<PackageSource> packageSources)
        {
            _ = packageSources ?? throw new ArgumentNullException(nameof(packageSources));

            if (name is null)
            {
                return null;
            }

            string trimmedName = name?.Trim() ?? string.Empty;

            List<PackageSource> existingPackageSource = packageSources
                .Where(packageSource =>
                    string.Equals(packageSource.Name, trimmedName, StringComparison.CurrentCultureIgnoreCase))
                .ToList();

            if (existingPackageSource.Count > 1)
            {
                throw new InvalidOperationException(message: Strings.Error_PackageSource_UniqueName);
            }

            return existingPackageSource.SingleOrDefault();
        }

        private static PackageSource? FindBySource(string source, List<PackageSource> packageSources)
        {
            _ = packageSources ?? throw new ArgumentNullException(nameof(packageSources));

            if (source is null)
            {
                return null;
            }

            string trimmedSource = source?.Trim() ?? string.Empty;

            List<PackageSource> existingPackageSource = packageSources
                .Where(packageSource =>
                {
                    string trimmedTargetSource = packageSource.Source?.Trim() ?? string.Empty;
                    bool areTrimmedStringsEqual =
                        string.Equals(
                            trimmedTargetSource,
                            trimmedSource,
                            StringComparison.OrdinalIgnoreCase)
                        && string.Equals(
                            PathValidator.GetCanonicalPath(trimmedTargetSource),
                            PathValidator.GetCanonicalPath(trimmedSource),
                            StringComparison.OrdinalIgnoreCase);
                    return areTrimmedStringsEqual;
                })
                .ToList();

            if (existingPackageSource.Count > 1)
            {
                throw new InvalidOperationException(message: Strings.Error_PackageSource_UniqueSource);
            }

            return existingPackageSource.SingleOrDefault();
        }
    }
}
