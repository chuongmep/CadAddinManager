using System.IO;
using System.Net;
using System.Reflection;
using System.Reflection.Emit;
using System.Windows;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Runtime;
using CadAddinManager.Model;
using CadAddinManager.View;
using CadAddinManager.ViewModel;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;
using Exception = System.Exception;

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


    public void RunActiveCommand()
    {
        AssemLoader assemLoader = new AssemLoader();
        var filePath = _activeCmd.FilePath;
        if (!File.Exists(filePath))
        {
            MessageBox.Show("File not found: " + filePath,DefaultSetting.AppName, MessageBoxButton.OK, MessageBoxImage.Error);
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
                        string fullName = string.Join(".", methodInfo.DeclaringType.Name,methodInfo.Name);
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
        _activeApp = null;
        _activeAppItem = null;
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

    public Addin ActiveApp
    {
        get => _activeApp;
        set => _activeApp = value;
    }

    public AddinItem ActiveAppItem
    {
        get => _activeAppItem;
        set => _activeAppItem = value;
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

    private Addin _activeApp;

    private AddinItem _activeAppItem;

    private AddinManager _addinManager;
}