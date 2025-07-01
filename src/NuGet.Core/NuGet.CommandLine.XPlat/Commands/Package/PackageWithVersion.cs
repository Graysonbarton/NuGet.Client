// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#nullable enable

using System;
using System.Collections.Generic;
using System.CommandLine.Parsing;
using NuGet.Versioning;

namespace NuGet.CommandLine.XPlat.Commands.Package
{
    internal record PackageWithVersion
    {
        public required string Id { get; init; }
        public required VersionRange? VersionRange { get; init; }

        internal static IReadOnlyList<PackageWithVersion> Parse(ArgumentResult result)
        {
            if (result.Tokens.Count == 0)
            {
                return [];
            }

            List<PackageWithVersion> packages = new List<PackageWithVersion>(result.Tokens.Count);

            foreach (var token in result.Tokens)
            {
                var package = ParseSingle(token.Value);
                packages.Add(package);
            }

            return packages;
        }

        internal static PackageWithVersion ParseSingle(ArgumentResult result)
        {
            // As long as the argument was defined with Arity == 1, System.CommandLine should handle errors when there are zero or more than one.
            var token = result.Tokens[0];
            return ParseSingle(token.Value);
        }

        internal static PackageWithVersion ParseSingle(string token)
        {
            string? packageId;
            VersionRange? newVersion;
            int separatorIndex = token.IndexOf('@');
            if (separatorIndex < 0)
            {
                packageId = token;
                newVersion = null;
            }
            else
            {
                packageId = token.Substring(0, separatorIndex);
                string versionString = token.Substring(separatorIndex + 1);
                if (string.IsNullOrEmpty(versionString))
                {
                    throw new Exception(Messages.Error_MissingVersion(token));
                }
                if (!VersionRange.TryParse(versionString, out newVersion))
                {
                    throw new Exception(Messages.Error_InvalidVersionRange(versionString));
                }
            }

            var package = new PackageWithVersion
            {
                Id = packageId,
                VersionRange = newVersion
            };
            return package;
        }
    }
}
