using System.Windows.Controls;
using CadAddinManager.ViewModel;

namespace CadAddinManager.View.Control
{
    /// <summary>
    /// Interaction logic for LogControl.xaml
    /// </summary>
    public partial class LogControl : UserControl
    {
        public LogControl()
        {
            InitializeComponent();
            LogControlViewModel viewModel = new LogControlViewModel();
            DataContext = viewModel;
            viewModel.FrmLogControl = this;
            this.Loaded += viewModel.LogFileWatcher;
            this.Unloaded += viewModel.UserControl_Unloaded;

        }
        
    }
}
