using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.Utilities.Collections;

internal partial class Build
{
    private Target Cleaning => _ => _
         .Executes(() =>
         {
             ArtifactsDirectory.CreateOrCleanDirectory();

             if (IsServerBuild) return;
             foreach (var projectName in Projects)
             {
                 var project = BuilderExtensions.GetProject(Solution, projectName);
                 var binDirectory = (AbsolutePath)new DirectoryInfo(project.GetBinDirectory()).FullName;
                 binDirectory.GlobDirectories($"{AddInBinPrefix}*", "Release*").ForEach(x => x.DeleteDirectory());
             }
         });
}