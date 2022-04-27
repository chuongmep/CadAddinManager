using System.Reflection;
using System.Reflection.Emit;
using System.Windows;
using Autodesk.AutoCAD.ApplicationServices;
using CadAddinManager.Model;
using CadAddinManager.View;
using CadAddinManager.ViewModel;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;

namespace CadAddinManager.Command;

public sealed class AddinManagerBase
{
    public void ExecuteCommand(bool faceless)
    {
        var vm = new AddInManagerViewModel();
        if (_activeCmd != null && faceless)
        {
            RunActiveCommand(vm);
            return;
        }

        _activeEc = null;
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


    public void RunActiveCommand(AddInManagerViewModel vm)
    {
        var filePath = _activeCmd.FilePath;
        try
        {
            vm.AssemLoader.HookAssemblyResolve();
            var assembly = vm.AssemLoader.LoadAddinsToTempFolder(filePath, false);
            if (assembly == null) return;
            else
            {
                _activeTempFolder = vm.AssemLoader.TempFolder;
                Type[] types = assembly.GetTypes();
                foreach (Type type in types)
                {
                    List<MethodInfo> methodInfos = type.GetMethods().ToList();
                    foreach (MethodInfo methodInfo in methodInfos)
                    {
                        var commandAtt = methodInfo
                            .GetCustomAttributes(typeof(Autodesk.AutoCAD.Runtime.CommandMethodAttribute), false)
                            .FirstOrDefault();
                        string name2 = $"{methodInfo.DeclaringType.Name}.{methodInfo.Name}";
                        if (_activeEc != null)
                        {
                            string name1 = $"{_activeEc.DeclaringType.Name}.{_activeEc.Name}";
                            if (name1 == name2)
                            {
                                Invoke(methodInfo);
                            }
                        }

                        string s = vm.SelectedCommandItem?.Name;
                        if (commandAtt != null && _activeEc == null && name2 == s)
                        {
                            _activeEc = methodInfo;
                            Invoke(_activeEc);
                        }
                    }
                }
            }
        }
        catch (Exception e)
        {
            MessageBox.Show(e.ToString());
        }
        finally
        {
            vm.AssemLoader.UnhookAssemblyResolve();
            vm.AssemLoader.CopyGeneratedFilesBack();
        }
    }

    void Invoke(MethodInfo methodInfo)
    {
        _activeEc = methodInfo;
        Document doc = Application.DocumentManager.MdiActiveDocument;
        using (doc.LockDocument())
        {
            if (_activeEc.IsStatic)
            {
                _activeEc.Invoke(null, null);
                return;
            }

            var instance = Activator.CreateInstance(_activeEc.DeclaringType);
            _activeEc.Invoke(instance, null);
            //TODO : Bi trung attribute : 
            // https://adndevblog.typepad.com/autocad/2014/01/detecting-net-command-duplicates-programmatically.html               
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

    public MethodInfo ActiveEC
    {
        get => _activeEc;
        set => _activeEc = value;
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

    private MethodInfo _activeEc;

    private Addin _activeCmd;

    private AddinItem _activeCmdItem;

    private Addin _activeApp;

    private AddinItem _activeAppItem;

    private AddinManager _addinManager;
}