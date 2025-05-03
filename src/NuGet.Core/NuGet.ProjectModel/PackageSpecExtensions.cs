// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using NuGet.Frameworks;

namespace NuGet.ProjectModel
{
    public static class PackageSpecExtensions
    {
        /// <summary>
        /// Get the nearest framework available in the project.
        /// </summary>
        public static TargetFrameworkInformation GetTargetFramework(this PackageSpec project, NuGetFramework targetFramework)
        {
            var frameworkInfo = project.TargetFrameworks.FirstOrDefault(f => NuGetFramework.Comparer.Equals(targetFramework, f.FrameworkName));
            if (frameworkInfo == null)
            {
                frameworkInfo = NuGetFrameworkUtility.GetNearest(project.TargetFrameworks,
                    targetFramework,
                    item => item.FrameworkName);
            }

            return frameworkInfo ?? new TargetFrameworkInformation();
        }
    }
}
