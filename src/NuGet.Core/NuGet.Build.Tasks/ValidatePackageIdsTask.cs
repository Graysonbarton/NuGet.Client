// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using NuGet.Packaging;

public class ValidatePackageIdsTask : Task
{
    [Required]
    public ITaskItem[] PackageReferences { get; set; }

    public override bool Execute()
    {
        bool success = true;

        foreach (var pkgRef in PackageReferences)
        {
            string packageId = pkgRef.ItemSpec;
            try
            {
                PackageIdValidator.ValidatePackageId(packageId);
            }
            catch (ArgumentException ex)
            {
                Log.LogError($"Invalid package ID '{packageId}': {ex.Message}");
                success = false;
            }
        }

        return success;
    }
}
