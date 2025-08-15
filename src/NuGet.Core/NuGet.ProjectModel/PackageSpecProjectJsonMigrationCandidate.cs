// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using NuGet.LibraryModel;

namespace NuGet.ProjectModel
{
    internal class PackageSpecProjectJsonMigrationCandidate
    {
        /// <summary>
        /// List of dependencies that apply to all frameworks.
        /// <see cref="ProjectStyle.PackageReference"/> based projects must not use this list and instead use the one in the <see cref="PackageSpec.TargetFrameworks"/> property which is a list of the <see cref="TargetFrameworkInformation"/> type.
        /// </summary>
        public IList<LibraryDependency> Dependencies { get; set; }
    }
}
