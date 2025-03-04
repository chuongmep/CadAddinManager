using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using WixSharp;
using WixSharp.CommonTasks;
using WixSharp.Controls;
using File = WixSharp.File;


const string BundleName = "CadAddinManager.bundle";
// string rootDirectory = Path.GetPathRoot(Environment.SystemDirectory);
string installationDir = Path.Combine(@"C:\ProgramData\\Autodesk\\ApplicationPlugins", BundleName);
const string projectName = "CadAddinManager";
const string outputName = "CadAddinManager";
const string outputDir = "output";
const string version = "2.0.3";
var fileName = new StringBuilder().Append(outputName).Append("-").Append(version);
var project = new Project
{
    Name = projectName,
    OutDir = outputDir,
    Platform = Platform.x64,
    Description = "Project Support Developer Work With Autocad And Civil3D API",
    UI = WUI.WixUI_InstallDir,
    Version = new Version(version),
    OutFileName = fileName.ToString(),
    InstallScope = InstallScope.perUser,
    MajorUpgrade = MajorUpgrade.Default,
    GUID = new Guid("D43065BC-D897-4C99-8876-C1D6B908133E"),
    BackgroundImage = @"Installer\Resources\Icons\BackgroundImage.png",
    BannerImage = @"Installer\Resources\Icons\BannerImage.png",
    ControlPanelInfo =
    {
        Manufacturer = "ChuongMep.com",
        HelpLink = "https://github.com/chuongmep/CadAddInManager/issues",
        Comments = "Project Support Developer With Autocad And Civil3D API",
        ProductIcon = @"Installer\Resources\Icons\ShellIcon.ico"
    },
    Dirs = new Dir[]
    {
        new InstallDir(Path.Combine(installationDir), GenerateWixEntities()),
    }
};

MajorUpgrade.Default.AllowSameVersionUpgrades = true;
project.RemoveDialogsBetween(NativeDialogs.WelcomeDlg, NativeDialogs.InstallDirDlg);
project.BuildMsi();

WixEntity[] GenerateWixEntities()
{
    Console.WriteLine("Start Create Installer");
    var versionRegex = new Regex(@"\d+");
    var versionStorages = new Dictionary<string, List<WixEntity>>();
    if (args.Length == 0) Console.WriteLine("Have some Problem with args build installer");
    int countEntity = 0;
    foreach (var directory in args)
    {
        Console.WriteLine($"Working with Directory: {directory}");
        var directoryInfo = new DirectoryInfo(directory);
        var fileVersion = versionRegex.Match(directoryInfo.Name).Value;
        var files = new Files($@"{directory}\*.*");
        if (versionStorages.TryGetValue(fileVersion, out var storage))
            storage.Add(files);
        else
            versionStorages.Add(fileVersion, new List<WixEntity> {files});
        var assemblies = Directory.GetFiles(directory, "*", SearchOption.AllDirectories);
        Console.WriteLine($"Adding '{fileVersion}' version files: ");
        foreach (var assembly in assemblies)
        {
            Console.WriteLine($"'{assembly}'");
            countEntity++;
        }
    }

    Console.WriteLine($"Added {countEntity} files to msi");
    WixEntity[] wixEntities = versionStorages.Select(storage => new Dir(storage.Key, storage.Value.ToArray()))
        .Cast<WixEntity>().ToArray();
    string dirXml = Path.Combine(Directory.GetCurrentDirectory(), projectName);
    string PackageContentsXml = Path.Combine(dirXml, "PackageContents.xml");
    Console.WriteLine($"add PackageContents.xml to wixEntities:{PackageContentsXml}");
    // add PackageContents.xml to wixEntities not include directory
    wixEntities = wixEntities.Append(new File(PackageContentsXml)).ToArray();
    return wixEntities;
}