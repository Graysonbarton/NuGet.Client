// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#nullable enable

using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Completions;
using System.CommandLine.Parsing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NuGet.CommandLine.XPlat.Commands.Package.Update;
using NuGet.Versioning;

namespace NuGet.CommandLine.XPlat.Commands.Package.Add;

internal class PackageAddCommand
{
    internal static void Register(Command packageCommand, Command? addCommand, Option<bool> interactiveOption)
    {
        Register(
            packageCommand,
            addCommand,
            interactiveOption,
            () => new CommandOutputLogger(Common.LogLevel.Information) { HidePrefixForInfoAndMinimal = true },
            () => new AddPackageReferenceCommandRunner(),
            () => new DGSpecFactory());
    }

    internal static void Register(
        Command packageCommand,
        Command? addCommand,
        Option<bool> interactiveOption,
        Func<ILoggerWithColor> getLogger,
        Func<IPackageReferenceCommandRunner> getCommandRunner,
        Func<IDGSpecFactory> getDgSpecFactory)
    {
        Option<bool> prereleaseOption = new Option<bool>("--prerelease")
        {
            Description = Strings.AddPkg_PackagePrerelease,
            Arity = ArgumentArity.Zero
        };

        Option<string> projectOption = new Option<string>("--project")
        {
            DefaultValueFactory = _ => Environment.CurrentDirectory,
            Description = Strings.ProjectArgumentDescription
        };

        Argument<PackageWithVersion> cmdPackageArgument = new Argument<PackageWithVersion>("packageId")
        {
            Description = Strings.AddPkg_PackageIdDescription,
            Arity = ArgumentArity.ExactlyOne,
            HelpName = "PACKAGE_ID",
            CustomParser = PackageWithVersion.ParseSingle
        };
        cmdPackageArgument.CompletionSources.Add((context) =>
        {
            // we should take --prerelease flags into account for version completion
            bool allowPrerelease = context.ParseResult.GetValue(prereleaseOption);

            string? projectDirectory = context.ParseResult.GetValue(projectOption) ?? Environment.CurrentDirectory;
            if (!Directory.Exists(projectDirectory))
            {
                projectDirectory = Path.GetDirectoryName(projectDirectory);
                if (string.IsNullOrEmpty(projectDirectory) || !Directory.Exists(projectDirectory))
                {
                    projectDirectory = Environment.CurrentDirectory;
                }
            }

            return QueryNuGetAsync(context.WordToComplete, allowPrerelease, projectDirectory).Result.Select(packageId => new CompletionItem(packageId));
        });

        Option<string> versionOption = new Option<string>("--version", "-v")
        {
            Description = Strings.AddPkg_PackageVersionDescription,
        };
        versionOption.CompletionSources.Add((context) =>
        {
            // we can only do version completion if we have a package id
            if (context.ParseResult.GetValue(cmdPackageArgument) is { VersionRange: null } packageId)
            {
                // we should take --prerelease flags into account for version completion
                var allowPrerelease = context.ParseResult.GetValue(prereleaseOption);

                string? projectDirectory = context.ParseResult.GetValue(projectOption) ?? Environment.CurrentDirectory;
                if (!string.IsNullOrEmpty(projectDirectory) && !Directory.Exists(projectDirectory))
                {
                    projectDirectory = Path.GetDirectoryName(projectDirectory);
                    if (string.IsNullOrEmpty(projectDirectory) || !Directory.Exists(projectDirectory))
                    {
                        projectDirectory = Environment.CurrentDirectory;
                    }
                }

                return QueryVersionsForPackage(packageId.Id, context.WordToComplete, allowPrerelease, projectDirectory)
                    .Result
                    .Select(version => new CompletionItem(version.ToNormalizedString()));
            }
            else
            {
                return [];
            }
        });
        versionOption.Validators.Add(optionResult => DisallowVersionIfPackageIdentityHasVersionValidator(optionResult, cmdPackageArgument));

        Option<string[]> frameworkOption = new Option<string[]>("--framework", "-f")
        {
            Description = Strings.AddPkg_FrameworksDescription,
            Arity = ArgumentArity.ZeroOrMore
        };

        Option<bool> noRestoreOption = new("--no-restore", "-n")
        {
            Description = Strings.AddPkg_NoRestoreDescription,
            Arity = ArgumentArity.Zero
        };

        Option<string[]> sourceOption = new Option<string[]>("--source", "-s")
        {
            Description = Strings.AddPkg_SourcesDescription,
            Arity = ArgumentArity.ZeroOrMore
        };

        Option<string> packageDirOption = new Option<string>("--package-directory")
        {
            Description = Strings.AddPkg_PackageDirectoryDescription,
        };

        Command command = new("add", Strings.AddPkg_Description);

        command.Validators.Add(commandResult => DisallowVersionIfPrereleaseOptionUsedValidator(commandResult, cmdPackageArgument, versionOption, prereleaseOption));
        command.Arguments.Add(cmdPackageArgument);
        command.Options.Add(versionOption);
        command.Options.Add(frameworkOption);
        command.Options.Add(noRestoreOption);
        command.Options.Add(sourceOption);
        command.Options.Add(packageDirOption);
        command.Options.Add(interactiveOption);
        command.Options.Add(prereleaseOption);
        command.Options.Add(projectOption);

        command.SetAction(async (parseResult, cancellationToken) =>
        {
            string[]? framework = parseResult.GetValue(frameworkOption);
            bool interactive = parseResult.GetValue(interactiveOption);
            ILoggerWithColor logger = new CommandOutputLogger(Common.LogLevel.Information)
            {
                HidePrefixForInfoAndMinimal = true
            };
            bool noRestore = parseResult.GetValue(noRestoreOption);
            string? packageDirectory = parseResult.GetValue(packageDirOption);
            PackageWithVersion package = parseResult.GetValue(cmdPackageArgument)!;
            bool prerelease = parseResult.GetValue(prereleaseOption);
            string projectPath = parseResult.GetValue(projectOption) ?? Directory.GetCurrentDirectory();
            string[]? sources = parseResult.GetValue(sourceOption);

            var args = new PackageReferenceArgs
            {
                Frameworks = framework,
                Interactive = interactive,
                Logger = logger,
                NoRestore = noRestore,
                PackageDirectory = packageDirectory,
                Package = package,
                Prerelease = prerelease,
                ProjectPath = projectPath,
                Sources = sources,
            };
            IPackageReferenceCommandRunner runner = getCommandRunner();
            IDGSpecFactory dGSpecFactory = getDgSpecFactory();
            return await runner.ExecuteCommand(args, new MSBuildAPIUtility(logger), dGSpecFactory);
        });

        packageCommand.Subcommands.Add(command);

        if (addCommand != null)
        {
            if (addCommand.Arguments.Count != 1)
            {
                throw new InvalidOperationException("The add command should have one argument, project.");
            }
            Argument<string> addCommandProjectArgument = (Argument<string>)addCommand.Arguments[0];

            Command hiddenCommand = new("package", Strings.AddPkg_Description);
            hiddenCommand.Validators.Add(commandResult => DisallowVersionIfPrereleaseOptionUsedValidator(commandResult, cmdPackageArgument, versionOption, prereleaseOption));
            hiddenCommand.Arguments.Add(cmdPackageArgument);
            hiddenCommand.Options.Add(versionOption);
            hiddenCommand.Options.Add(frameworkOption);
            hiddenCommand.Options.Add(noRestoreOption);
            hiddenCommand.Options.Add(sourceOption);
            hiddenCommand.Options.Add(packageDirOption);
            hiddenCommand.Options.Add(interactiveOption);
            hiddenCommand.Options.Add(prereleaseOption);
            hiddenCommand.Options.Add(projectOption);

            hiddenCommand.SetAction(async (parseResult, cancellationToken) =>
            {
                // this command can be called with an argument or an option for the project path - we prefer the option.
                // if the option is not present, we use the argument value instead.
                string project;
                if (parseResult.GetResult(projectOption) is OptionResult or && !or.Implicit)
                {
                    project = parseResult.GetValue(projectOption)!;
                }
                else
                {
                    project = parseResult.GetValue(addCommandProjectArgument) ?? Directory.GetCurrentDirectory();
                }
                string[]? framework = parseResult.GetValue(frameworkOption);
                bool interactive = parseResult.GetValue(interactiveOption);
                ILoggerWithColor logger = new CommandOutputLogger(Common.LogLevel.Information)
                {
                    HidePrefixForInfoAndMinimal = true
                };
                bool noRestore = parseResult.GetValue(noRestoreOption);
                string? packageDirectory = parseResult.GetValue(packageDirOption);
                PackageWithVersion package = parseResult.GetValue(cmdPackageArgument)!;
                bool prerelease = parseResult.GetValue(prereleaseOption);
                string projectPath = parseResult.GetValue(projectOption) ?? Directory.GetCurrentDirectory();
                string[]? sources = parseResult.GetValue(sourceOption);

                var args = new PackageReferenceArgs
                {
                    Frameworks = framework,
                    Interactive = interactive,
                    Logger = logger,
                    NoRestore = noRestore,
                    PackageDirectory = packageDirectory,
                    Package = package,
                    Prerelease = prerelease,
                    ProjectPath = projectPath,
                    Sources = sources,
                };
                IPackageReferenceCommandRunner runner = getCommandRunner();
                IDGSpecFactory dgSpecFactory = getDgSpecFactory();
                return await runner.ExecuteCommand(args, new MSBuildAPIUtility(logger), dgSpecFactory);
            });
            addCommand.Subcommands.Add(hiddenCommand);
        }
    }

    private static void DisallowVersionIfPackageIdentityHasVersionValidator(OptionResult result, Argument<PackageWithVersion> cmdPackageArgument)
    {
        if (result.Parent!.GetValue(cmdPackageArgument)!.VersionRange != null)
        {
            result.AddError(Strings.ValidationFailedDuplicateVersion);
        }
    }

    private static void DisallowVersionIfPrereleaseOptionUsedValidator(CommandResult result, Argument<PackageWithVersion> cmdPackageArgument, Option<string> versionOption, Option<bool> preReleaseOption)
    {
        var package = result.Parent!.GetValue(cmdPackageArgument);
        var version = result.Parent!.GetValue(versionOption);
        var prereleaseResult = result.Parent!.GetResult(preReleaseOption);
        if ((package?.VersionRange is not null || version is not null) && prereleaseResult?.Implicit == false)
        {
            result.AddError(Strings.Error_PrereleaseWhenVersionSpecified);
        }
    }

    public static async Task<IEnumerable<string>> QueryNuGetAsync(string packageStem, bool allowPrerelease, string configDirectory)
    {
        try
        {
            TabCompletion.Setup();
            var packages = await TabCompletion.GetPackageCompletionsAsync(packageStem, allowPrerelease, configDirectory);
            return packages;
        }
#pragma warning disable CA1031 // Do not catch general exception types
        // tab completion must neve throw
        catch (Exception)
#pragma warning restore CA1031 // Do not catch general exception types
        {
            return [];
        }
    }

    internal static async Task<IEnumerable<NuGetVersion>> QueryVersionsForPackage(string packageId, string versionFragment, bool allowPrerelease, string configDirectory)
    {
        try
        {
            TabCompletion.Setup();
            var versions = await TabCompletion.GetVersionsAsync(packageId, versionFragment, allowPrerelease, configDirectory);
            return versions;
        }
#pragma warning disable CA1031 // Do not catch general exception types
        // tab completion must neve throw
        catch (Exception)
#pragma warning restore CA1031 // Do not catch general exception types
        {
            return [];
        }
    }
}
