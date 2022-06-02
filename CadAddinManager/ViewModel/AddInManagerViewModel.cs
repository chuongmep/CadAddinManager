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
    private ObservableCollection<AddinModel> lispFunctionItems;

    public ObservableCollection<AddinModel> LispFunctionItems
    {
        get => lispFunctionItems;
        set
        {
            if (value == lispFunctionItems) return;
            lispFunctionItems = value;
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
    private AddinModel selectedLispItem;

    public AddinModel SelectedLispItem
    {
        get
        {
            if (selectedLispItem != null && selectedLispItem.IsParentTree == true && IsTabLispSelected)
            {
                MAddinManagerBase.ActiveLisp = selectedLispItem.Addin;
            }
            else if (selectedLispItem != null && selectedLispItem.IsParentTree == false && IsTabLispSelected)
            {
                MAddinManagerBase.ActiveLispItem = selectedLispItem.AddinItem;
                MAddinManagerBase.ActiveLisp = selectedLispItem.Addin;
                VendorDescription = MAddinManagerBase.ActiveLispItem.Description;
            }
            return selectedLispItem;
        }
        set => OnPropertyChanged(ref selectedLispItem, value);
    }

    public ICommand LoadCommand => new RelayCommand(LoadCommandClick);
    public ICommand ClearCommand => new RelayCommand(ClearCommandClick);

    public ICommand RemoveCommand => new RelayCommand(RemoveAddinClick);
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

    private ObservableCollection<CadAddin> addinStartup;

    public ObservableCollection<CadAddin> AddInStartUps
    {
        get { return addinStartup ??= new ObservableCollection<CadAddin>(); }
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
    private bool isTabLogSelected;

    public bool IsTabLogSelected
    {
        get
        {
            if (isTabLogSelected)
            {
                LogControlViewModel vm = new LogControlViewModel(){FrmLogControl = FrmAddInManager.LogControl};
                FrmAddInManager.LogControl.DataContext = vm;
                FrmAddInManager.LogControl.Loaded += vm.LogFileWatcher;
                FrmAddInManager.LogControl.Unloaded += vm.UserControl_Unloaded;
            };
            return isTabLogSelected;
        }
        set => OnPropertyChanged(ref isTabLogSelected, value);
    }
    private bool isTabAppSelected;

    public bool IsTabLispSelected
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

    private void HelpCommandClick()
    {
        Process.Start("https://github.com/chuongmep/CadAddInManager/wiki");
    }

    public AddInManagerViewModel()
    {
        AssemLoader = new AssemLoader();
        MAddinManagerBase = AddinManagerBase.Instance;
        CommandItems = FreshTreeItems(false, MAddinManagerBase.AddinManager.Commands);
        LispFunctionItems = FreshTreeItems(false, MAddinManagerBase.AddinManager.LispFunctions);
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

    public void ExecuteAddinCommandClick()
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
                }
            }
            if (SelectedLispItem?.IsParentTree == false && FrmAddInManager != null)
            {
                MAddinManagerBase.ActiveLisp = SelectedLispItem.Addin;
                MAddinManagerBase.ActiveLispItem = SelectedLispItem.AddinItem;
                CheckCountSelected(LispFunctionItems, out var result);
                if (result > 0)
                {
                    //TODO
                    // FrmAddInManager.Close();
                    // Execute();

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
        MAddinManagerBase.RunActiveCommand();
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
        switch (addinType)
        {
            case AddinType.Invalid:
                MessageBox.Show(Resource.LoadInvalid);
                return;
            case AddinType.Command:
                IsTabCmdSelected = true;
                FrmAddInManager.TabCommand.Focus();
                break;
            case AddinType.LispFunction:
                IsTabLispSelected = true;
                FrmAddInManager.TabLispFunction.Focus();
                break;
            case AddinType.Mixed:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
       
        MAddinManagerBase.AddinManager.SaveToAimIni();
        CommandItems = FreshTreeItems(false, MAddinManagerBase.AddinManager.Commands);
        LispFunctionItems = FreshTreeItems(false, MAddinManagerBase.AddinManager.LispFunctions);
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
            if (IsTabLispSelected)
            {
                foreach (var parent in LispFunctionItems)
                {
                    if (parent.IsInitiallySelected)
                    {
                        MAddinManagerBase.ActiveLisp = parent.Addin;
                        MAddinManagerBase.ActiveLispItem = parent.AddinItem;
                        if (MAddinManagerBase.ActiveLisp != null)
                        {
                            MAddinManagerBase.AddinManager.LispFunctions.RemoveAddIn(MAddinManagerBase.ActiveLisp);
                        }
                        MAddinManagerBase.ActiveLisp = null;
                        MAddinManagerBase.ActiveLispItem = null;
                        LispFunctionItems = FreshTreeItems(false, MAddinManagerBase.AddinManager.LispFunctions);
                        return;
                    }
                    foreach (var addinChild in parent.Children)
                    {
                        if (addinChild.IsInitiallySelected)
                        {
                            //Set Value to run for add-in command
                            MAddinManagerBase.ActiveLisp = parent.Addin;
                            MAddinManagerBase.ActiveLispItem = addinChild.AddinItem;
                        }
                    }
                }

                if (MAddinManagerBase.ActiveLispItem != null)
                {
                    MAddinManagerBase.ActiveLisp.RemoveItem(MAddinManagerBase.ActiveLispItem);
                    MAddinManagerBase.ActiveLisp = null;
                    MAddinManagerBase.ActiveLispItem = null;
                }
                LispFunctionItems = FreshTreeItems(false, MAddinManagerBase.AddinManager.LispFunctions);
            }
            //Save All SetTings
            MAddinManagerBase.AddinManager.SaveToAimIni();
        }
        catch (Exception e)
        {
            MessageBox.Show(e.ToString());
        }
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