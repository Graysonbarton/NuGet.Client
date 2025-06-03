// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using NuGet.Configuration;

namespace NuGet.PackageManagement.VisualStudio.Options
{
    public static class PackageSourceValidator
    {
        public static void ValidatePathOrThrow(PackageSource packageSource)
        {
            if (packageSource is null)
            {
                throw new ArgumentNullException(nameof(packageSource));
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

        public static void PrepareForSave(List<PackageSource> packageSources)
        {
            if (packageSources is null)
            {
                throw new ArgumentNullException(nameof(packageSources));
            }

            ValidateUniquenessOrThrow(packageSources);
        }

        public static void ValidateUniquenessOrThrow(List<PackageSource> targetPackageSources)
        {
            EnsureUniqueNames(targetPackageSources);
            EnsureUniqueSources(targetPackageSources);
        }

        private static void EnsureUniqueNames(List<PackageSource> targetPackageSources)
        {
            var seen = new HashSet<string>(StringComparer.CurrentCultureIgnoreCase);

            foreach (PackageSource packageSource in targetPackageSources)
            {
                if (!seen.Add(packageSource.Name))
                {
                    throw new ArgumentException(Strings.Error_PackageSource_UniqueName);
                }
            }
        }

        private static void EnsureUniqueSources(List<PackageSource> targetPackageSources)
        {
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (PackageSource packageSource in targetPackageSources)
            {
                if (!seen.Add(packageSource.Source)
                    || !seen.Add(PathValidator.GetCanonicalPath(packageSource.Source)))
                {
                    throw new ArgumentException(Strings.Error_PackageSource_UniqueSource);
                }
            }
        }

        public static PackageSource? FindByName(PackageSource targetPackageSource, List<PackageSource> existingPackageSources)
        {
            if (targetPackageSource is null || existingPackageSources is null)
            {
                return null;
            }

            PackageSource existingPackageSource = existingPackageSources
                .SingleOrDefault(packageSource =>
                    string.Equals(packageSource.Name, targetPackageSource.Name, StringComparison.CurrentCultureIgnoreCase));

            return existingPackageSource;
        }

        public static PackageSource? FindBySource(PackageSource targetPackageSource, List<PackageSource> existingPackageSources)
        {
            if (targetPackageSource is null || existingPackageSources is null)
            {
                return null;
            }

            PackageSource existingPackageSource = existingPackageSources
                .SingleOrDefault(packageSource =>
                    string.Equals(packageSource.Source, targetPackageSource.Source, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(
                        PathValidator.GetCanonicalPath(packageSource.Source),
                        PathValidator.GetCanonicalPath(targetPackageSource.Source),
                        StringComparison.OrdinalIgnoreCase));

            return existingPackageSource;
        }
    }
}
