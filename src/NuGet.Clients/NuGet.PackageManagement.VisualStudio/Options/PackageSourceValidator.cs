// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
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
            ValidatePathOrThrow(packageSource);

            return packageSource;
        }

        internal static void ValidatePathOrThrow(PackageSource packageSource)
        {
            _ = packageSource ?? throw new ArgumentNullException(nameof(packageSource));

            if (packageSource.IsHttp)
            {
                return;
            }

            string source = packageSource.Source;

            if (!Common.PathValidator.IsValidLocalPath(source) &&
                !Common.PathValidator.IsValidUncPath(source) &&
                !Common.PathValidator.IsValidUrl(source))
            {
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
                if (!seen.Add(packageSource.Name))
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
                //TODO this is off because canonical path is being seen as a duplicate.
                if (!seen.Add(packageSource.Source)
                    || !seen.Add(PathValidator.GetCanonicalPath(packageSource.Source)))
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

            PackageSource existingPackageSource = packageSources
                .SingleOrDefault(packageSource =>
                    string.Equals(packageSource.Name, name, StringComparison.CurrentCultureIgnoreCase));

            return existingPackageSource;
        }

        private static PackageSource? FindBySource(string source, List<PackageSource> packageSources)
        {
            _ = packageSources ?? throw new ArgumentNullException(nameof(packageSources));

            if (source is null)
            {
                return null;
            }

            PackageSource existingPackageSource = packageSources
                .SingleOrDefault(packageSource =>
                    string.Equals(packageSource.Source, source, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(
                        PathValidator.GetCanonicalPath(packageSource.Source),
                        PathValidator.GetCanonicalPath(source),
                        StringComparison.OrdinalIgnoreCase));

            return existingPackageSource;
        }
    }
}
