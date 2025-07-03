// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#nullable enable

using System;
using System.Diagnostics;
using System.IO;
using NuGet.Common;
using NuGet.ProjectModel;

namespace NuGet.CommandLine.XPlat.Commands.Package.Update
{
    internal class DGSpecFactory : IDGSpecFactory
    {
        IEnvironmentVariableReader _environmentVariableReader;

        public DGSpecFactory()
        {
            _environmentVariableReader = new EnvironmentVariableWrapper();
        }

        public DependencyGraphSpec? GetDependencyGraphSpec(string project)
        {
            string tempFile = Path.GetTempFileName();
            try
            {
                if (!RunMsbuildTarget(project, tempFile))
                {
                    return null;
                }

                DependencyGraphSpec result = DependencyGraphSpec.Load(tempFile);

                return result;
            }
            finally
            {
                File.Delete(tempFile);
            }
        }

        private bool RunMsbuildTarget(string project, string tempFile)
        {
            string dotnetPath = GetDotnetPath();

            // don't redirect stdout or stderr, so errors are output. But use quiet verbosity, so that success has no output.
            ProcessStartInfo processStartInfo = new ProcessStartInfo(dotnetPath)
            {
                Arguments = $"msbuild " +
                $"\"{project}\" " +
                $"-restore:false " +
                $"-target:GenerateRestoreGraphFile " +
                $"-property:RestoreGraphOutputPath=\"{tempFile}\" " +
                $"-property:RestoreRecursive=false " +
                $"-nologo " +
                $"-verbosity:quiet " +
                $"-tl:false " +
                $"-noautoresponse",
                UseShellExecute = false,
            };

            using var process = Process.Start(processStartInfo)!;
            process.WaitForExit();

            return process.ExitCode == 0;

            string GetDotnetPath()
            {
                // Check if running in the main dotnet CLI process (command registered by NuGetCommands.Add(RootCommand)).
                string? processPath = Environment.ProcessPath;
                if (!string.IsNullOrEmpty(processPath) && Path.GetFileNameWithoutExtension(processPath).Equals("dotnet", StringComparison.OrdinalIgnoreCase))
                {
                    return processPath;
                }

                // When the .NET SDK runs NuGet.CommandLine.XPlat as a child process, it sets the DOTNET_HOST_PATH environment variable.
                processPath = _environmentVariableReader.GetEnvironmentVariable("DOTNET_HOST_PATH");
                if (!string.IsNullOrEmpty(processPath))
                {
                    return processPath;
                }

                // Check if DOTNET_ROOT environment variable is set
                processPath = _environmentVariableReader.GetEnvironmentVariable("DOTNET_ROOT");
                if (!string.IsNullOrEmpty(processPath))
                {
                    // If DOTNET_ROOT is set, assume dotnet is in the root directory.
                    string dotnetExecutable = Path.Combine(processPath, "dotnet");
                    if (File.Exists(dotnetExecutable))
                    {
                        return dotnetExecutable;
                    }
                }

                // If all else fails, just hope that 'dotnet' is in the PATH.
                return "dotnet";
            }
        }
    }
}
