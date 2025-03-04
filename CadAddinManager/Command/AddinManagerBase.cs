using System.IO;
using System.Reflection;
using System.Windows;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Runtime;
using CadAddinManager.Model;
using CadAddinManager.View;
using CadAddinManager.ViewModel;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;
using Exception = System.Exception;
using MessageBox = System.Windows.MessageBox;

namespace CadAddinManager.Command;

public sealed class AddinManagerBase
{
    public void ExecuteCommand(bool faceless)
    {
        var vm = new AddInManagerViewModel();
        if (_activeCmd != null && faceless)
        {
            RunActiveCommand();
            return;
        }

        var FrmAddInManager = new FrmAddInManager(vm);
        FrmAddInManager.WindowStartupLocation = WindowStartupLocation.CenterScreen;
        // Application.ShowModelessWindow(FrmAddInManager);
        FrmAddInManager.SetCadAsWindowOwner();
        FrmAddInManager.Show();
    }

    public string ActiveTempFolder
    {
        get => _activeTempFolder;
        set => _activeTempFolder = value;
    }

//
// #if A25
//     public void RunActiveCommand()
//     {
//         var filePath = _activeCmd.FilePath;
//         if (!File.Exists(filePath))
//         {
//             MessageBox.Show("File not found: " + filePath, DefaultSetting.AppName, MessageBoxButton.OK,
//                 MessageBoxImage.Error);
//             return;
//         }
//         var alc = new AssemblyLoadContext(filePath);
//         Stream stream = null;
//         try
//         {
//             
//             
//             stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
//             Assembly assembly = alc.LoadFromStream(stream);
//             //var assembly = assemLoader.LoadAddinsToTempFolder(filePath, false);
//             WeakReference alcWeakRef = new WeakReference(alc, trackResurrection: true);
//             Type[] types = assembly.GetTypes();
//             foreach (Type type in types)
//             {
//                 List<MethodInfo> methodInfos = type.GetMethods().ToList();
//                 foreach (MethodInfo methodInfo in methodInfos)
//                 {
//                     CommandMethodAttribute commandAtt = (CommandMethodAttribute)methodInfo
//                         .GetCustomAttributes(typeof(CommandMethodAttribute), false)
//                         .FirstOrDefault();
//                     string fullName = string.Join(".", methodInfo.DeclaringType.Name, methodInfo.Name);
//                     if (commandAtt != null && fullName == Instance.ActiveCmdItem.FullClassName)
//                     {
//                         // clone the assembly to 
//                         // DO HERE
//                         Invoke(methodInfo);
//                         alc.Unload();
//                     }
//                 }
//             }
//             int counter = 0;
//             for (counter = 0; alcWeakRef.IsAlive && (counter < 10); counter++)
//             {
//                 alc = null;
//                 GC.Collect();
//                 GC.WaitForPendingFinalizers();
//             }
//             stream.Close();
//         }
//         catch (Exception e)
//         {
//             //Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage(e.ToString());
//             alc?.Unload();
//             WeakReference alcWeakRef = new WeakReference(alc, trackResurrection: true);
//             for (int counter = 0; alcWeakRef.IsAlive && (counter < 10); counter++)
//             {
//                 alc = null;
//                 GC.Collect();
//                 GC.WaitForPendingFinalizers();
//             }
//             stream?.Close();
//         }
//         // finally
//         // {
//         //     assemLoader.UnhookAssemblyResolve();
//         //     assemLoader.CopyGeneratedFilesBack();
//         // }
//     }
// #else
     public void RunActiveCommand()
    {
        AssemLoader assemLoader = new AssemLoader();
        var filePath = _activeCmd.FilePath;
        if (!File.Exists(filePath))
        {
            MessageBox.Show("File not found: " + filePath, DefaultSetting.AppName, MessageBoxButton.OK,
                MessageBoxImage.Error);
            return;
        }

        try
        {
            assemLoader.HookAssemblyResolve();
            var assembly = assemLoader.LoadAddinsToTempFolder(filePath, false);
            if (assembly == null) return;
            else
            {
                Type[] types = assembly.GetTypes();
                foreach (Type type in types)
                {
                    List<MethodInfo> methodInfos = type.GetMethods().ToList();
                    foreach (MethodInfo methodInfo in methodInfos)
                    {
                        CommandMethodAttribute commandAtt = (CommandMethodAttribute) methodInfo
                            .GetCustomAttributes(typeof(CommandMethodAttribute), false)
                            .FirstOrDefault();
                        string fullName = string.Join(".", methodInfo.DeclaringType.Name, methodInfo.Name);
                        if (commandAtt != null && fullName == Instance.ActiveCmdItem.FullClassName)
                        {
                            Invoke(methodInfo);
                        }
                    }
                }
            }
        }
        catch (Exception e)
        {
            Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage(e.ToString());
        }
        finally
        {
            assemLoader.UnhookAssemblyResolve();
            assemLoader.CopyGeneratedFilesBack();
        }
    }


    void Invoke(MethodInfo methodInfo)
    {
        Document doc = Application.DocumentManager.MdiActiveDocument;
        using (doc.LockDocument())
        {
            if (methodInfo.IsStatic)
            {
                methodInfo.Invoke(null, null);
                return;
            }

            try
            {
                if (methodInfo.DeclaringType != null)
                {
                    var instance = Activator.CreateInstance(methodInfo.DeclaringType);
                    methodInfo.Invoke(instance, null);
                }
            }
            catch (System.Reflection.TargetInvocationException e)
            {
                doc.Editor.WriteMessage(e.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
            }
        }
    }

    public static AddinManagerBase Instance
    {
        get
        {
            if (_instance == null)
            {
#pragma warning disable RCS1059 // Avoid locking on publicly accessible instance.
                lock (typeof(AddinManagerBase))
                {
                    if (_instance == null)
                    {
                        _instance = new AddinManagerBase();
                    }
                }
#pragma warning restore RCS1059 // Avoid locking on publicly accessible instance.
            }

            return _instance;
        }
    }

    private AddinManagerBase()
    {
        _addinManager = new AddinManager();
        _activeCmd = null;
        _activeCmdItem = null;
        _activeLisp = null;
        _activeLispItem = null;
    }

    public Addin ActiveCmd
    {
        get => _activeCmd;
        set => _activeCmd = value;
    }

    public AddinItem ActiveCmdItem
    {
        get => _activeCmdItem;
        set => _activeCmdItem = value;
    }

    public Addin ActiveLisp
    {
        get => _activeLisp;
        set => _activeLisp = value;
    }

    public AddinItem ActiveLispItem
    {
        get => _activeLispItem;
        set => _activeLispItem = value;
    }

    public AddinManager AddinManager
    {
        get => _addinManager;
        set => _addinManager = value;
    }

    private string _activeTempFolder = string.Empty;

    private static volatile AddinManagerBase _instance;


    private Addin _activeCmd;

    private AddinItem _activeCmdItem;

    private Addin _activeLisp;

    private AddinItem _activeLispItem;

    private AddinManager _addinManager;
}