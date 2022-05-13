using System.Windows;

namespace CadAddinManager.Model;

public static class StaticUtil
{
    public static void ShowWarning(string msg)
    {
        MessageBox.Show(msg, DefaultSetting.AppName, MessageBoxButton.OK, MessageBoxImage.Exclamation);
    }
    /// <summary>
    /// CaseInsensitiveContains
    /// </summary>
    /// <param name="text"></param>
    /// <param name="value"></param>
    /// <param name="stringComparison"></param>
    /// <returns></returns>
    public static bool CaseInsensitiveContains(this string text, string value, StringComparison stringComparison = StringComparison.CurrentCultureIgnoreCase)
    {
        return text.IndexOf(value, stringComparison) >= 0;
    }

}