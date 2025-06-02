// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Threading.Tasks;
using NuGet.CommandLine.XPlat.Commands.Package.Update;
using NuGet.Commands;
using NuGet.Configuration;
using NuGet.ProjectModel;
using NuGet.Protocol.Core.Types;

namespace NuGet.CommandLine.XPlat.Utility
{
    internal static class ImplicitRestore
    {
        public static async Task RestoreAsync(string path, ILoggerWithColor logger)
        {
            DGSpecFactory dGSpecFactory = new DGSpecFactory();
            DependencyGraphSpec dgspec = dGSpecFactory.GetDependencyGraphSpec(path);

            if (dgspec == null)
            {
                return;
            }

            var providers = new List<IPreLoadedRestoreRequestProvider>
            {
                new DependencyGraphSpecRequestProvider(new RestoreCommandProvidersCache(), dgspec)
            };

            var restoreContext = new RestoreArgs()
            {
                CacheContext = new SourceCacheContext(),
                LockFileVersion = LockFileFormat.Version,
                Log = logger,
                MachineWideSettings = new XPlatMachineWideSetting(),
                PreLoadedRequestProviders = providers,
            };

            await RestoreRunner.RunAsync(restoreContext);
        }
    }

}
