using System;
using System.Collections.Generic;
using System.Linq;

using Nuke.Common;
using Nuke.Common.CI;
using Nuke.Common.Execution;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.GitVersion;
using Nuke.Common.Utilities.Collections;

using static Nuke.Common.EnvironmentInfo;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.IO.PathConstruction;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

class Build : NukeBuild {
    /// Support plugins are available for:
    ///   - JetBrains ReSharper        https://nuke.build/resharper
    ///   - JetBrains Rider            https://nuke.build/rider
    ///   - Microsoft VisualStudio     https://nuke.build/visualstudio
    ///   - Microsoft VSCode           https://nuke.build/vscode
    public static int Main() => Execute<Build>(x => x.Compile);

    [Parameter("Output")] readonly string Output = RootDirectory / "bin";

    [Parameter("PluginName")] readonly string PluginName;

    [GitVersion] readonly GitVersion GitVersion;

    readonly IEnumerable<RevitConfiguration> DebugConfigurations = RevitConfiguration.GetDebugConfiguration();
    readonly IEnumerable<RevitConfiguration> ReleaseConfigurations = RevitConfiguration.GetReleaseConfiguration();

    AbsolutePath SourceDirectory => RootDirectory / PluginName;

    Target Clean => _ => _
        .Requires(() => PluginName)
        .Executes(() => {
            SourceDirectory.GlobDirectories("**/bin", "**/obj").ForEach(DeleteDirectory);
            EnsureCleanDirectory(Output);
        });

    Target FullClean => _ => _
        .Executes(() => {
            SourceDirectory.GlobDirectories("**/bin", "**/obj")
                .Where(item => item != (SourceDirectory / "build" / "bin"))
                .ForEach(DeleteDirectory);
            EnsureCleanDirectory(Output);
        });

    Target Compile => _ => _
        .DependsOn(Clean)
        .Requires(() => Output)
        .Requires(() => PluginName)
        .Executes(() => {
            DotNetBuild(s => s
                .DisableNoRestore()
                .SetProjectFile(PluginName)
                .SetOutputDirectory(Output)
                .CombineWith(DebugConfigurations, (settings, config) => settings
                    .SetConfiguration(config)
                    .SetSimpleVersion(GitVersion, config)));
        });

    Target FullCompile => _ => _
        .DependsOn(FullClean)
        .Requires(() => Output)
        .Executes(() => {
            DotNetBuild(s => s
                .DisableNoRestore()
                .SetOutputDirectory(Output)
                .CombineWith(DebugConfigurations, (settings, config) => settings
                    .SetConfiguration(config)
                    .SetSimpleVersion(GitVersion, config)));
        });

    Target Publish => _ => _
        .DependsOn(Clean)
        .Requires(() => Output)
        .Requires(() => PluginName)
        .Executes(() => {
            DotNetBuild(s => s
                .DisableNoRestore()
                .SetProjectFile(PluginName)
                .SetOutputDirectory(Output)
                .CombineWith(ReleaseConfigurations, (settings, config) => settings
                    .SetConfiguration(config)
                    .SetSimpleVersion(GitVersion, config)));
        });

    Target FullPublish => _ => _
        .DependsOn(FullClean)
        .Requires(() => Output)
        .Executes(() => {
            DotNetBuild(s => s
                .DisableNoRestore()
                .SetOutputDirectory(Output)
                .CombineWith(ReleaseConfigurations, (settings, config) => settings
                        .SetConfiguration(config)
                        .SetSimpleVersion(GitVersion, config)));
        });
}

static class VersioningExtensions {
    public static DotNetBuildSettings SetSimpleVersion(this DotNetBuildSettings settings,
        GitVersion gitVersion,
        RevitConfiguration configuration) {
        return settings
            .SetCopyright($"Copyright © {DateTime.Now.Year}")
            .SetAssemblyVersion(InjectRevitVersion(configuration, gitVersion.AssemblySemVer))
            .SetFileVersion(InjectRevitVersion(configuration, gitVersion.AssemblySemFileVer))
            .SetInformationalVersion(InjectRevitVersion(configuration, gitVersion.InformationalVersion));
    }

    public static string InjectRevitVersion(RevitConfiguration configuration, string versionString) {
        int index = versionString.IndexOf('.');
        return configuration.Version + versionString.Substring(index);
    }
}