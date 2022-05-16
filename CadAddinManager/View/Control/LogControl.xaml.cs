using System.Text;
using System.Windows.Forms;
using System.Windows.Input;
using CadAddinManager.Model;
using CadAddinManager.ViewModel;
using UserControl = System.Windows.Controls.UserControl;

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
         private void RightClickCopyCmdExecuted(object sender, ExecutedRoutedEventArgs e)
                {
                    StringBuilder sb = new StringBuilder();
                    foreach (LogMessageString dd in listBox_LogMessages.SelectedItems)
                    {
                        sb.AppendLine(dd.Message);
                    }
                    Clipboard.SetText(sb.ToString());
                }
                private void RightClickCopyCmdCanExecute(object sender, CanExecuteRoutedEventArgs e)
                {
                    e.CanExecute = listBox_LogMessages.SelectedItem != null;
                }
    }
}
