// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Build.Evaluation;
using NuGet.Commands;
using NuGet.Common;
using NuGet.Configuration;
using NuGet.ProjectModel;
using NuGet.Protocol.Core.Types;

namespace NuGet.CommandLine.XPlat.Utility
{
    internal class ImplicitRestoringCommand
    {
        public static async Task RunRestore(string projectPath, ILogger logger)
        {
            var projectCollection = new ProjectCollection();
            var dgSpecFile = Path.GetTempFileName();
            DependencyGraphSpec dgSpec = null;

            try
            {
                var globalProperties = new Dictionary<string, string>()
                {
                    ["RestoreGraphOutputPath"] = dgSpecFile,
                    ["RestoreDotnetCliToolReferences"] = "false",
                    ["RestoreRecursive"] = "false"
                };

                var project = projectCollection.LoadProject(projectPath, globalProperties, null);

                if (!project.Build("GenerateRestoreGraphFile"))
                {
                    throw new Exception("Generating DGSpec failed");
                }

                dgSpec = DependencyGraphSpec.Load(dgSpecFile);
            }
            finally
            {
                File.Delete(dgSpecFile);
            }

            if (dgSpec == null)
            {
                throw new Exception("Failed to load DependencyGraphSpec.");
            }

            var providerCache = new RestoreCommandProvidersCache();

            using (var cacheContext = new SourceCacheContext())
            {
                cacheContext.NoCache = false;
                cacheContext.IgnoreFailedSources = false;

                var providers = new List<IPreLoadedRestoreRequestProvider>
                {
                    new DependencyGraphSpecRequestProvider(providerCache, dgSpec)
                };

                var restoreContext = new RestoreArgs()
                {
                    CacheContext = cacheContext,
                    LockFileVersion = LockFileFormat.Version,
                    Log = logger,
                    MachineWideSettings = new XPlatMachineWideSetting(),
                    PreLoadedRequestProviders = providers
                };

                var restoreResult = await RestoreRunner.RunAsync(restoreContext);
            }
        }
    }
}
