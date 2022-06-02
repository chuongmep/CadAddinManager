using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using CadAddinManager.ViewModel;

namespace CadAddinManager.View;

/// <summary>
/// Interaction logic for FrmAddInManager.xaml
/// </summary>
public partial class FrmAddInManager : Window
{
    private readonly AddInManagerViewModel viewModel;

    public FrmAddInManager(AddInManagerViewModel vm)
    {
        InitializeComponent();
        DataContext = vm;
        viewModel = vm;
        vm.FrmAddInManager = this;
        this.Closing += FrmAddInManager_Closing;
    }

    private void FrmAddInManager_Closing(object sender, CancelEventArgs e)
    {
        viewModel.FrmAddInManager = null;
    }

    private void TbxDescription_OnLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
    {
        if (viewModel.MAddinManagerBase.ActiveCmdItem != null && TabControl.SelectedIndex == 0)
        {
            viewModel.MAddinManagerBase.ActiveCmdItem.Description = TbxDescription.Text;
        }
        viewModel.MAddinManagerBase.AddinManager.SaveToAimIni();
    }
    private void HandleTextboxKeyPress(object sender, KeyEventArgs e)
    {
        if(e.Key == Key.Down)
        {
            if (viewModel.IsTabCmdSelected)
            {
                TreeViewCommand.Focus();
            }
            if (viewModel.IsTabLispSelected)
            {
                TreeViewLispFunction.Focus();
            }
            else
            {
                LogControl.Focus();
            }
        }
        
    }
    private void HandleTreeViewCommandKeyPress(object sender, KeyEventArgs e)
    {
        int indexCmd = TreeViewCommand.Items.IndexOf(TreeViewCommand.SelectedItem);
        if (e.Key == Key.Up && TabCommand.IsFocused)
        {
            tbxSearch.Focus();
        }
        else if (e.Key == Key.Up && indexCmd==0 && TabCommand.IsSelected)
        {
            TabCommand.Focus();
        }
        if (e.Key == Key.Down && TabCommand.IsSelected)
        {
            TreeViewCommand.Focus();
        }
        if (e.Key == Key.Enter)
        {
            viewModel.ExecuteAddinCommandClick();
        }

    }
    private void HandleTreeViewLispKeyPress(object sender, KeyEventArgs e)
    {
        int indexCmd = TreeViewLispFunction.Items.IndexOf(TreeViewLispFunction.SelectedItem);
        if (e.Key == Key.Up && TabLispFunction.IsFocused)
        {
            tbxSearch.Focus();
        }
        else if (e.Key == Key.Up && indexCmd==0 && TabLispFunction.IsSelected)
        {
            TabLispFunction.Focus();
        }
        if (e.Key == Key.Down && TabLispFunction.IsSelected)
        {
            TreeViewLispFunction.Focus();
        }
        if (e.Key == Key.Enter)
        {
            viewModel.ExecuteAddinCommandClick();
        }

    }
    private void CloseFormEvent(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape) Close();
    }
}