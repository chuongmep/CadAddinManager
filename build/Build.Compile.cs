using Nuke.Common;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.MSBuild;
using static Nuke.Common.Tools.MSBuild.MSBuildTasks;

internal partial class Build
{
    private Target Compile => _ => _
         .TriggeredBy(Cleaning)
         .Executes(() =>
         {
             var configurations = GetConfigurations(BuildConfiguration, InstallerConfiguration);
             configurations.ForEach(configuration =>
             {
                 MSBuild(s =>
                 {
                     var settings = s
                         .SetTargets("Rebuild")
                         .SetConfiguration(configuration)
                         .SetVerbosity(MSBuildVerbosity.Minimal)
                         .DisableNodeReuse()
                         .EnableRestore();
                     
                     // Only set custom MSBuild path if it's available (local builds)
                     if (!string.IsNullOrEmpty(MsBuildPath.Value))
                         settings = settings.SetProcessToolPath(MsBuildPath.Value);
                     
                     return settings;
                 });
             });
         });
}