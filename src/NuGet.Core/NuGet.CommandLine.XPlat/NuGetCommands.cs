// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#nullable enable

using System;
using System.CommandLine;
using System.Linq;
using NuGet.CommandLine.XPlat.Commands.Package.Add;
using NuGet.CommandLine.XPlat.Commands.Package.Update;

namespace NuGet.CommandLine.XPlat;

/// <summary>
/// Class used by the .NET SDK to register NuGet's commands into the dotnet CLI.
/// </summary>
public static class NuGetCommands
{
    /// <summary>
    /// <para>Adds NuGet's dotnet CLI commands to the dotnet CLI RootCommand object</para>
    /// </summary>
    /// <param name="rootCommand">The CLI's RootCommand instance</param>
    /// <param name="interactiveOption">Reuse the interactive option from the dotnet CLI to avoid reimplementing their detection of CI pipelines.</param>
    /// <remarks>Many of NuGet's commands are defined in the dotnet/sdk repo, and those run NuGet.CommandLine.XPlat.dll as a child process.
    /// Those commands are not added by this method.</remarks>
    public static void Add(RootCommand rootCommand, Option<bool> interactiveOption)
    {
        var packageCommand = rootCommand.Subcommands.FirstOrDefault(c => c.Name == "package");
        if (packageCommand is null)
        {
            packageCommand = new Command("package");
            rootCommand.Subcommands.Add(packageCommand);
        }
        else
        {
            var existingPackageAddCommand = packageCommand.Subcommands.FirstOrDefault(c => c.Name == "add");
            if (existingPackageAddCommand is not null)
            {
                packageCommand.Subcommands.Remove(existingPackageAddCommand);
            }
        }

        var addCommand = rootCommand.Subcommands.FirstOrDefault(c => c.Name == "add");
        if (addCommand is not null)
        {
            var existingAddPackageCommand = addCommand.Subcommands.FirstOrDefault(c => c.Name == "package");
            if (existingAddPackageCommand is not null)
            {
                addCommand.Subcommands.Remove(existingAddPackageCommand);
            }
        }

        PackageUpdateCommand.Register(packageCommand);
        PackageAddCommand.Register(packageCommand, addCommand, interactiveOption);
    }

    // Remove after dotnet/sdk updated to use the documented overload.
    public static void Add(RootCommand rootCommand)
    {
        var interactiveOption = new Option<bool>("--interactive")
        {
            Description = Strings.AddPkg_InteractiveDescription,
            DefaultValueFactory = _ => !Console.IsOutputRedirected
        };

        Add(rootCommand, interactiveOption);
    }
}
