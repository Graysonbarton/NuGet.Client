// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#nullable enable

using NuGet.CommandLine.XPlat.Commands.Package;
using NuGet.Common;

namespace NuGet.CommandLine.XPlat
{
    internal class PackageReferenceArgs
    {
        public required string ProjectPath { get; init; }
        public required ILogger Logger { get; init; }
        public string[]? Frameworks { get; init; }
        public string[]? Sources { get; set; }
        public string? PackageDirectory { get; set; }
        public required bool NoRestore { get; init; }
        public required bool Interactive { get; init; }
        public required bool Prerelease { get; init; }
        public required PackageWithVersion Package { get; init; }
    }
}
