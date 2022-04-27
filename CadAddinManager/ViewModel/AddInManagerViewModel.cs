using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Input;
using CadAddinManager.Command;
using CadAddinManager.Model;
using CadAddinManager.View;
using CadAddinManager.View.Control;
using Microsoft.Win32;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;

namespace CadAddinManager.ViewModel;

public class AddInManagerViewModel : ViewModelBase
{
    //public AddInPlugin ExternalCommandData { get; set; }
    public FrmAddInManager FrmAddInManager { get; set; }
    public AssemLoader AssemLoader { get; set; }

    public AddinManagerBase MAddinManagerBase { get; set; }

    private ObservableCollection<AddinModel> commandItems;

    public ObservableCollection<AddinModel> CommandItems
    {
        get => commandItems;
        set
        {
            if (value == commandItems) return;
            commandItems = value;
            OnPropertyChanged();
        }
    }

    private AddinModel selectedCommandItem;

    public AddinModel SelectedCommandItem
    {
        get
        {
            if (selectedCommandItem != null && selectedCommandItem.IsParentTree == true && IsTabCmdSelected)
            {
                IsCanRun = false;
                MAddinManagerBase.ActiveCmd = selectedCommandItem.Addin;
            }
            else if (selectedCommandItem != null && selectedCommandItem.IsParentTree == false && IsTabCmdSelected)
            {
                IsCanRun = true;
                MAddinManagerBase.ActiveCmdItem = selectedCommandItem.AddinItem;
                MAddinManagerBase.ActiveCmd = selectedCommandItem.Addin;
                VendorDescription = MAddinManagerBase.ActiveCmdItem.Description;
            }
            else IsCanRun = false;
            return selectedCommandItem;
        }
        set => OnPropertyChanged(ref selectedCommandItem, value);
    }

    private AddinModel selectedAppItem;

    public AddinModel SelectedAppItem
    {
        get
        {
            if (selectedAppItem != null && selectedAppItem.IsParentTree == true && IsTabAppSelected)
            {
                MAddinManagerBase.ActiveApp = selectedAppItem.Addin;
            }
            else if (selectedAppItem != null && selectedAppItem.IsParentTree == false && IsTabAppSelected)
            {
                MAddinManagerBase.ActiveAppItem = selectedAppItem.AddinItem;
                MAddinManagerBase.ActiveApp = selectedAppItem.Addin;
                VendorDescription = MAddinManagerBase.ActiveAppItem.Description;
            }
            return selectedAppItem;
        }
        set => OnPropertyChanged(ref selectedAppItem, value);
    }

    public ICommand LoadCommand => new RelayCommand(LoadCommandClick);
    public ICommand ClearCommand => new RelayCommand(ClearCommandClick);

    public ICommand RemoveCommand => new RelayCommand(RemoveAddinClick);
    public ICommand SaveCommandFolder => new RelayCommand(SaveCommandLocalFolder);


    private readonly ICommand _executeAddinCommand = null;
    public ICommand ExecuteAddinCommand => _executeAddinCommand ?? new RelayCommand(ExecuteAddinCommandClick);
    public ICommand OpenLcAssemblyCommand => new RelayCommand(OpenLcAssemblyCommandClick);
    public ICommand ReloadCommand => new RelayCommand(ReloadCommandClick);
    public ICommand FreshSearch => new RelayCommand(FreshSearchClick);

    private string searchText;

    public string SearchText
    {
        get
        {
            FreshSearchClick();
            return searchText;
        }
        set => OnPropertyChanged(ref searchText, value);
    }

    private bool isCurrentVersion = true;

    public bool IsCurrentVersion
    {
        get => isCurrentVersion;
        set => OnPropertyChanged(ref isCurrentVersion, value);
    }

    private ObservableCollection<NavisAddin> addinStartup;

    public ObservableCollection<NavisAddin> AddInStartUps
    {
        get { return addinStartup ??= new ObservableCollection<NavisAddin>(); }
        set => OnPropertyChanged(ref addinStartup, value);
    }

    public ICommand HelpCommand => new RelayCommand(HelpCommandClick);

    private string vendorDescription = string.Empty;

    public string VendorDescription
    {
        get => vendorDescription;
        set => OnPropertyChanged(ref vendorDescription, value);
    }

    private bool isTabCmdSelected = true;

    public bool IsTabCmdSelected
    {
        get => isTabCmdSelected;
        set => OnPropertyChanged(ref isTabCmdSelected, value);
    }

    private bool isTabAppSelected;

    public bool IsTabAppSelected
    {
        get
        {
            if (isTabAppSelected) IsCanRun = false;
            return isTabAppSelected;
        }
        set => OnPropertyChanged(ref isTabAppSelected, value);
    }

    private bool isCanRun;

    public bool IsCanRun
    {
        get => isCanRun;
        set => OnPropertyChanged(ref isCanRun, value);
    }

    private bool isTabStartSelected;

    public bool IsTabStartSelected
    {
        get
        {
            if (isTabStartSelected) IsCanRun = false;
            return isTabStartSelected;
        }
        set => OnPropertyChanged(ref isTabStartSelected, value);
    }

    private void HelpCommandClick()
    {
        Process.Start("https://github.com/chuongmep/CadAddInManager/wiki");
    }

    public AddInManagerViewModel()
    {
        AssemLoader = new AssemLoader();
        MAddinManagerBase = AddinManagerBase.Instance;
        CommandItems = FreshTreeItems(false, MAddinManagerBase.AddinManager.Commands);
    }

    private ObservableCollection<AddinModel> FreshTreeItems(bool isSearchText, Addins addins)
    {
        var MainTrees = new ObservableCollection<AddinModel>();
        foreach (var keyValuePair in addins.AddinDict)
        {
            var addin = keyValuePair.Value;
            var title = keyValuePair.Key;
            var addinItemList = addin.ItemList;
            var addinModels = new List<AddinModel>();
            foreach (var addinItem in addinItemList)
            {
                if (isSearchText)
                {
                    if (addinItem.FullClassName.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        addinModels.Add(new AddinModel(addinItem.FullClassName)
                        {
                            IsChecked = true,
                            Addin = addin,
                            AddinItem = addinItem,
                        });
                    }
                }
                else
                {
                    addinModels.Add(new AddinModel(addinItem.FullClassName)
                    {
                        IsChecked = true,
                        Addin = addin,
                        AddinItem = addinItem,
                    });
                }
            }
            var root = new AddinModel(title)
            {
                IsChecked = true,
                Children = addinModels,
                IsParentTree = true,
                Addin = addin,
            };
            root.Initialize();
            MainTrees.Add(root);
        }

        return MainTrees;
    }

    private void ExecuteAddinCommandClick()
    {
        try
        {
            if (SelectedCommandItem?.IsParentTree == false && FrmAddInManager != null)
            {
                MAddinManagerBase.ActiveCmd = SelectedCommandItem.Addin;
                MAddinManagerBase.ActiveCmdItem = SelectedCommandItem.AddinItem;
                CheckCountSelected(CommandItems, out var result);
                if (result > 0)
                {
                    FrmAddInManager.Close();
                    Execute();
                    //FrmAddInManager = null;

                }
            }
        }
        catch (Exception e)
        {
            MessageBox.Show(e.ToString());
        }
    }

    private void Execute()
    {
        MAddinManagerBase.RunActiveCommand(this);
    }

    private void OpenLcAssemblyCommandClick()
    {
        bool flag = MAddinManagerBase.ActiveCmd == null;
        if (flag) return;
        string path = MAddinManagerBase.ActiveCmd.FilePath;
        if (!File.Exists(path))
        {
            ShowFileNotExit(path);
            return;
        }
        Process.Start("explorer.exe", "/select, " + path);
    }

    private void OpenLcAssemblyAppClick()
    {
        bool flag = MAddinManagerBase.ActiveApp == null;
        if (flag) return;
        string path = MAddinManagerBase.ActiveApp.FilePath;
        if (!File.Exists(path))
        {
            ShowFileNotExit(path);
            return;
        }
        Process.Start("explorer.exe", "/select, " + path);
    }

    private void ShowFileNotExit(string path)
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine(Resource.FileNotExit);
        sb.AppendLine("Path :");
        sb.AppendLine(path);
        MessageBox.Show(sb.ToString(), DefaultSetting.AppName, MessageBoxButton.OK, MessageBoxImage.Exclamation);
    }

    private void CheckCountSelected(ObservableCollection<AddinModel> addinModels, out int result)
    {
        result = 0;
        foreach (var addinModel in addinModels)
        {
            if (addinModel.IsInitiallySelected) result++;
            foreach (var modelChild in addinModel.Children)
            {
                if (modelChild.IsInitiallySelected) result++;
            }
        }
    }

    private void LoadCommandClick()
    {
        OpenFileDialog openFileDialog = new OpenFileDialog();
        openFileDialog.Filter = @"assembly files (*.dll)|*.dll|All files (*.*)|*.*";
        if (openFileDialog.ShowDialog() != true)
        {
            return;
        }
        var fileName = openFileDialog.FileName;
        if (!File.Exists(fileName)) return;
        LoadAssemblyCommand(fileName);
    }

    private void ReloadCommandClick()
    {
        if (SelectedCommandItem == null)
        {
            SortedDictionary<string, Addin> Commands = MAddinManagerBase.AddinManager.Commands.AddinDict;
            SortedDictionary<string, Addin> OldCommands = new SortedDictionary<string, Addin>(Commands);
            foreach (var Command in OldCommands.Values)
            {
                string fileName = Command.FilePath;
                if (File.Exists(fileName)) MAddinManagerBase.AddinManager.LoadAddin(fileName, AssemLoader);
            }
            MAddinManagerBase.AddinManager.SaveToAimIni();
            CommandItems = FreshTreeItems(false, MAddinManagerBase.AddinManager.Commands);
            return;
        }
        bool flag = MAddinManagerBase.ActiveCmd == null;
        if (flag) return;
        string path = MAddinManagerBase.ActiveCmd.FilePath;
        if (!File.Exists(path)) return;
        LoadAssemblyCommand(path);
    }

    private void LoadAssemblyCommand(string fileName)
    {
        var addinType = MAddinManagerBase.AddinManager.LoadAddin(fileName, AssemLoader);
        if (addinType == AddinType.Invalid)
        {
            MessageBox.Show(Resource.LoadInvalid);
            return;
        }
        
        IsTabCmdSelected = true;
        FrmAddInManager.TabCommand.Focus();
        MAddinManagerBase.AddinManager.SaveToAimIni();
        CommandItems = FreshTreeItems(false, MAddinManagerBase.AddinManager.Commands);
    }
    private void RemoveAddinClick()
    {
        try
        {
            if (IsTabCmdSelected)
            {
                foreach (var parent in CommandItems)
                {
                    if (parent.IsInitiallySelected)
                    {
                        MAddinManagerBase.ActiveCmd = parent.Addin;
                        MAddinManagerBase.ActiveCmdItem = parent.AddinItem;
                        if (MAddinManagerBase.ActiveCmd != null)
                        {
                            MAddinManagerBase.AddinManager.Commands.RemoveAddIn(MAddinManagerBase.ActiveCmd);
                        }
                        MAddinManagerBase.ActiveCmd = null;
                        MAddinManagerBase.ActiveCmdItem = null;
                        CommandItems = FreshTreeItems(false, MAddinManagerBase.AddinManager.Commands);
                        return;
                    }
                    foreach (var addinChild in parent.Children)
                    {
                        if (addinChild.IsInitiallySelected)
                        {
                            //Set Value to run for add-in command
                            MAddinManagerBase.ActiveCmd = parent.Addin;
                            MAddinManagerBase.ActiveCmdItem = addinChild.AddinItem;
                        }
                    }
                }

                if (MAddinManagerBase.ActiveCmdItem != null)
                {
                    MAddinManagerBase.ActiveCmd.RemoveItem(MAddinManagerBase.ActiveCmdItem);
                    MAddinManagerBase.ActiveCmd = null;
                    MAddinManagerBase.ActiveCmdItem = null;
                }
                CommandItems = FreshTreeItems(false, MAddinManagerBase.AddinManager.Commands);
            }
            //Save All SetTings
            MAddinManagerBase.AddinManager.SaveToAimIni();
        }
        catch (Exception e)
        {
            MessageBox.Show(e.ToString());
        }
    }

    private void SaveCommandLocalFolder()
    {
        SaveFileDialog dlg = new SaveFileDialog();
        dlg.Filter = "Navis Addin (*.addin)|*.addin";
        dlg.DefaultExt = "addin";
        dlg.AddExtension = true;
        dlg.Title = DefaultSetting.AppName;
        dlg.ShowDialog();
        if (!string.IsNullOrEmpty(dlg.FileName))
        {
            MAddinManagerBase.AddinManager.SaveAsLocal(this, dlg.FileName);
            ShowSuccessfully();
        }
    }

    private void ShowSuccessfully()
    {
        MessageBox.Show(FrmAddInManager, "Save Successfully", DefaultSetting.AppName, MessageBoxButton.OK, MessageBoxImage.Information);
        FrmAddInManager.Close();
    }

    private void FreshSearchClick()
    {
        var flag = string.IsNullOrEmpty(searchText);
        if (IsTabCmdSelected)
        {
            if (flag)
            {
                CommandItems = FreshTreeItems(false, MAddinManagerBase.AddinManager.Commands);
                return;
            }
            CommandItems = FreshTreeItems(true, MAddinManagerBase.AddinManager.Commands);
        }
    }
    private void ClearCommandClick()
    {
        var tempFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Temp", DefaultSetting.TempFolderName);
        if (Directory.Exists(tempFolder))
        {
            Process.Start(tempFolder);
        }
    }

}