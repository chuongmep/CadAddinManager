using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows;
using CadAddinManager.View;
using Mono.Cecil;
using Mono.Cecil.Pdb;

namespace CadAddinManager.Model;

public class AssemLoader
{
    public string OriginalFolder
    {
        get => originalFolder;
        set => originalFolder = value;
    }

    public string TempFolder
    {
        get => tempFolder;
        set => tempFolder = value;
    }

    public AssemLoader()
    {
        tempFolder = string.Empty;
        refedFolders = new List<string>();
        copiedFiles = new Dictionary<string, DateTime>();
    }

    public void CopyGeneratedFilesBack()
    {
        var files = Directory.GetFiles(tempFolder, "*.*", SearchOption.AllDirectories);
        if(!files.Any()) return;
        foreach (var text in files)
        {
            if (copiedFiles.ContainsKey(text))
            {
                var t = copiedFiles[text];
                var fileInfo = new FileInfo(text);
                if (fileInfo.LastWriteTime > t)
                {
                    var str = text.Remove(0, tempFolder.Length);
                    var destinationFilename = originalFolder + str;
                    FileUtils.CopyFile(text, destinationFilename);
                }
            }
            else
            {
                var str2 = text.Remove(0, tempFolder.Length);
                var destinationFilename2 = originalFolder + str2;
                FileUtils.CopyFile(text, destinationFilename2);
            }
        }
    }

    public void HookAssemblyResolve()
    {
        AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
    }

    public void UnhookAssemblyResolve()
    {
        AppDomain.CurrentDomain.AssemblyResolve -= CurrentDomain_AssemblyResolve;
    }

    public Assembly LoadAddinsToTempFolder(string originalFilePath, bool parsingOnly)
    {
        if (string.IsNullOrEmpty(originalFilePath)||
            !File.Exists(originalFilePath))
        {
            return null;
        }
        this.parsingOnly = parsingOnly;
        originalFolder = Path.GetDirectoryName(originalFilePath);
        var stringBuilder = new StringBuilder(Path.GetFileNameWithoutExtension(originalFilePath));
        if (parsingOnly)  stringBuilder.Append("-Parsing-");
        else stringBuilder.Append("-Executing-");
        tempFolder = FileUtils.CreateTempFolder(stringBuilder.ToString());
        string fileAssemblyTemp = ResolveDuplicateMethod(originalFilePath);
        var assembly = CopyAndLoadAddin(fileAssemblyTemp, parsingOnly);
        if (assembly == null || !IsAPIReferenced(assembly))
        {
            return null;
        }
        return assembly;
    }

    private string ResolveDuplicateMethod(string originalFilePath)
    {
        
        // AssemblyDefinition ass = AssemblyDefinition.ReadAssembly(originalFilePath);
        AssemblyDefinition ass = GetAssemblyDef(originalFilePath);
        foreach (ModuleDefinition def in ass.Modules)
        {
            foreach (TypeDefinition d in def.Types)
            {
                foreach (MethodDefinition m in d.Methods)
                {
                    if (!m.IsConstructor && !m.IsRuntimeSpecialName && m.Name != "Main")
                    {
                        foreach (CustomAttribute customAttribute in m.CustomAttributes)
                        {
                            if (customAttribute.Constructor.DeclaringType.Name == "CommandMethodAttribute")
                            {
                                int count = customAttribute.ConstructorArguments.Count;
                                CustomAttribute newAttr = null;
                                if (count == 3)
                                {
                                   newAttr = CreateCustomAttribute3Type(customAttribute);
                                }
                                else if (count == 2)
                                {
                                    newAttr = CreateCustomAttribute2Type(customAttribute);
                                }
                                else if (count == 1)
                                {
                                    newAttr = CreateCustomAttribute1Type(customAttribute);
                                }
                                m.CustomAttributes.Remove(customAttribute);
                                m.CustomAttributes.Add(newAttr);
                                break;
                            }
                        }
                    }
                }
            }
        }
        string fileAssemblyTemp = SaveAssemblyModifyToTemp(originalFilePath);
        ass.Write(fileAssemblyTemp);
        return fileAssemblyTemp;
    }
    public static AssemblyDefinition GetAssemblyDef(string assemblyPath)
    {
        
        var assemblyResolver = new DefaultAssemblyResolver();
        var assemblyLocation = Path.GetDirectoryName(assemblyPath);
        assemblyResolver.AddSearchDirectory(assemblyLocation);

        var readerParameters = new ReaderParameters { AssemblyResolver = assemblyResolver };

        var pdbName = Path.ChangeExtension(assemblyPath, "pdb");
        if (File.Exists(pdbName))
        {
            var symbolReaderProvider = new PdbReaderProvider();
            readerParameters.SymbolReaderProvider = symbolReaderProvider;
            readerParameters.ReadSymbols = true;
        }

        var assemblyDef = AssemblyDefinition.ReadAssembly(assemblyPath, readerParameters);
        return assemblyDef;
    }
    private string SaveAssemblyModifyToTemp(string originalFilePath)
    {
        string prefix = "RenameCommand-";
        var str = $"{DateTime.Now:yyyyMMdd_HHmmss_ffff}";
        var path = Path.Combine(DefaultSetting.DirLogFile, prefix + str);
        var directoryInfo3 = new DirectoryInfo(path);
        directoryInfo3.Create();
        string fileAssemblyTemp = Path.Combine(directoryInfo3.FullName, Path.GetFileName(originalFilePath));
        return fileAssemblyTemp;
    }
    private CustomAttribute CreateCustomAttribute3Type(CustomAttribute customAttribute)
    {
        string valueAttribute = ResolveValueAttribute(customAttribute);
        return new CustomAttribute(customAttribute.Constructor)
        { 
            ConstructorArguments = 
            {
                new CustomAttributeArgument(
                    customAttribute.ConstructorArguments[0].Type,valueAttribute),
                new CustomAttributeArgument(
                    customAttribute.ConstructorArguments[1].Type, 
                    customAttribute.ConstructorArguments[1].Value),
                new CustomAttributeArgument(
                    customAttribute.ConstructorArguments[2].Type, 
                    customAttribute.ConstructorArguments[2].Value),
            }
        };  
    }
    private CustomAttribute CreateCustomAttribute2Type(CustomAttribute customAttribute)
    {
        string valueAttribute = ResolveValueAttribute(customAttribute);
        return new CustomAttribute(customAttribute.Constructor)
        { 
            ConstructorArguments = 
            {
                new CustomAttributeArgument(
                    customAttribute.ConstructorArguments[0].Type, 
                    valueAttribute),
                new CustomAttributeArgument(
                    customAttribute.ConstructorArguments[1].Type, 
                    customAttribute.ConstructorArguments[1].Value),
            }
        };  
    }
    private CustomAttribute CreateCustomAttribute1Type(CustomAttribute customAttribute)
    {
        string valueAttribute = ResolveValueAttribute(customAttribute);

        return new CustomAttribute(customAttribute.Constructor)
        { 
            ConstructorArguments = 
            {
                new CustomAttributeArgument(
                    customAttribute.ConstructorArguments[0].Type,valueAttribute),
            }
        };  
    }
    private string ResolveValueAttribute(CustomAttribute att)
    {
        string suffix =  DateTime.Now.ToString("yyyyMMddhhmmssfff");
        string value = att.ConstructorArguments[0].Value +suffix;
        if (value.Length >= 64) value = "Execute" + suffix;
        return value;
    }
    private Assembly CopyAndLoadAddin(string srcFilePath, bool onlyCopyRelated)
    {
        var text = string.Empty;
        if (!FileUtils.FileExistsInFolder(srcFilePath, tempFolder))
        {
            var directoryName = Path.GetDirectoryName(srcFilePath);
            if (!refedFolders.Contains(directoryName))
            {
                refedFolders.Add(directoryName);
            }

            var list = new List<FileInfo>();
            text = FileUtils.CopyFileToFolder(srcFilePath, tempFolder, onlyCopyRelated, list);
            if (string.IsNullOrEmpty(text))
            {
                return null;
            }

            foreach (var fileInfo in list)
            {
                copiedFiles.Add(fileInfo.FullName, fileInfo.LastWriteTime);
            }
        }

        return LoadAddin(text);
    }

    private Assembly LoadAddin(string filePath)
    {
        Assembly result = null;
        try
        {
            Monitor.Enter(this);
            //Agree this error to load depend event assembly, see https://github.com/chuongmep/RevitAddInManager/issues/7
            result = Assembly.LoadFile(filePath);
        }
        finally
        {
            Monitor.Exit(this);
        }

        return result;
    }

    private Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
    {
        Assembly result;
        new AssemblyName(args.Name);
        var text = SearchAssemblyFileInTempFolder(args.Name);
        if (File.Exists(text))
        {
            result = LoadAddin(text);
        }
        else
        {
            text = SearchAssemblyFileInOriginalFolders(args.Name);
            if (string.IsNullOrEmpty(text))
            {
                var array = args.Name.Split(new char[]
                {
                    ','
                });
                var ass = array[0];
                if (array.Length > 1)
                {
                    var text3 = array[2];
                    if (ass.EndsWith(".resources", StringComparison.CurrentCultureIgnoreCase) &&
                        !text3.EndsWith("neutral", StringComparison.CurrentCultureIgnoreCase))
                    {
                        ass = ass.Substring(0, ass.Length - ".resources".Length);
                    }
                    // Skip searching for the assembly if assembly with specified name is already loaded
                    foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                    {
                        if (String.Compare(assembly.GetName().Name, ass, StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            return null;
                        }
                    }
                    text = SearchAssemblyFileInTempFolder(ass);
                    if (File.Exists(text))
                    {
                        return LoadAddin(text);
                    }

                    text = SearchAssemblyFileInOriginalFolders(ass);
                }
            }

            if (string.IsNullOrEmpty(text))
            {
                var loader = new AssemblyLoader(args.Name);
                loader.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                if (loader.ShowDialog() != true)
                {
                    return null;
                }

                text = loader.resultPath;
            }

            result = CopyAndLoadAddin(text, true);
        }

        return result;
    }

    private string SearchAssemblyFileInTempFolder(string assemName)
    {
        try
        {
            var array = new string[] {".dll", ".exe"};
            var text = string.Empty;
            var strLength = assemName.IndexOf(',');
            var str = strLength == -1 ? assemName : assemName.Substring(0, strLength);
            foreach (var str2 in array)
            {
                text = tempFolder + "\\" + str + str2;

                if (File.Exists(text))
                {
                    return text;
                }
            }
        }
        catch (Exception e)
        {
            throw new ArgumentException(e.ToString());
        }

        return string.Empty;
    }

    private string SearchAssemblyFileInOriginalFolders(string assemName)
    {
        var array = new string[]
        {
            ".dll",
            ".exe"
        };
        string text;
        // Avoid ArgumentOutOfRangeException from .Substring() by checking length parameter
        var strLength = assemName.IndexOf(',');
        var text2 = strLength == -1 ? assemName : assemName.Substring(0, strLength);
        // var text2 = assemName.Substring(0, assemName.IndexOf(','));
        foreach (var str in array)
        {
            text = dotnetDir + "\\" + text2 + str;
            if (File.Exists(text))
            {
                return text;
            }
        }

        foreach (var str2 in array)
        {
            foreach (var str3 in refedFolders)
            {
                text = str3 + "\\" + text2 + str2;
                if (File.Exists(text))
                {
                    return text;
                }
            }
        }

        try
        {
            var directoryInfo =
                new DirectoryInfo(Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName));
            var path = directoryInfo.Parent?.FullName + "\\Regression\\_RegressionTools\\";
            if (Directory.Exists(path))
            {
                foreach (var text3 in Directory.GetFiles(path, "*.*", SearchOption.AllDirectories))
                {
                    if (Path.GetFileNameWithoutExtension(text3).Equals(text2, StringComparison.OrdinalIgnoreCase))
                    {
                        return text3;
                    }
                }
            }
        }
        catch (Exception e)
        {
            throw new ArgumentException(e.ToString());
        }

        try
        {
            var num = assemName.IndexOf("XMLSerializers", StringComparison.OrdinalIgnoreCase);
            if (num != -1)
            {
                assemName = "System.XML" + assemName.Substring(num + "XMLSerializers".Length);
                return SearchAssemblyFileInOriginalFolders(assemName);
            }
        }
        catch (Exception e)
        {
            throw new ArgumentException(e.ToString());
        }

        return null;
    }

    /// <summary>
    /// Check api is in autocad
    /// </summary>
    /// <param name="assembly"></param>
    /// <returns></returns>
    private bool IsAPIReferenced(Assembly assembly)
    {
       
        if (assembly == null) return false;
        if (string.IsNullOrEmpty(CadApiAssemblyFullName))
        {
            foreach (var assembly2 in AppDomain.CurrentDomain.GetAssemblies())
            {
               
                if (String.Compare(assembly2.GetName().Name.ToLower(), "accoremgd", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    CadApiAssemblyFullName = assembly2.GetName().Name;
                    break;
                }

                if (String.Compare(assembly2.GetName().Name.ToLower(), "acdbmgd", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    CadApiAssemblyFullName = assembly2.GetName().Name;
                    break;
                }

                if (String.Compare(assembly2.GetName().Name.ToLower(), "mscorlib", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    CadApiAssemblyFullName = assembly2.GetName().Name;
                    break;
                }
                if (String.Compare(assembly2.GetName().Name.ToLower(), "fabricationapi", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    CadApiAssemblyFullName = assembly2.GetName().Name;
                    break;
                }
            }
        }
     
        if (CadApiAssemblyFullName == "mscorlib") return true;
        foreach (var assemblyName in assembly.GetReferencedAssemblies())
        {
            if (CadApiAssemblyFullName == assemblyName.Name)
            {
                return true;
            }
        }

        return false;
    }

    private readonly List<string> refedFolders;

    private readonly Dictionary<string, DateTime> copiedFiles;

    private bool parsingOnly;

    private string originalFolder;

    private string tempFolder;

    private static string dotnetDir =
        Environment.GetEnvironmentVariable("windir") + "\\Microsoft.NET\\Framework\\v2.0.50727";

    public static string ResolvedAssemPath = string.Empty;

    private string CadApiAssemblyFullName;
}