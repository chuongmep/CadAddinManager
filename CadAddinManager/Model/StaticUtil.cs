using System.Windows;

namespace CadAddinManager.Model;

public static class StaticUtil
{
    public static void ShowWarning(string msg)
    {
        MessageBox.Show(msg, DefaultSetting.AppName, MessageBoxButton.OK, MessageBoxImage.Exclamation);
    }

    public static string CommandFullName = "Hello"; //typeof(MethodInfo).FullName;
}