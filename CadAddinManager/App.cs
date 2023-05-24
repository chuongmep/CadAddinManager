using System.Windows.Media.Imaging;
using Autodesk.AutoCAD.Runtime;
using Autodesk.Windows;
using CadAddinManager.Command;
using CadAddinManager.View.Control;
using ImageSource = System.Windows.Media.ImageSource;
using Orientation = System.Windows.Controls.Orientation;

namespace CadAddinManager;

public class App : ICadCommand
{
    public const string RibbonTitle = "AddinManager";
    public const string RibbonId = "AddinManager";

    [CommandMethod("InitAddinManager")]
    public override void Execute()
    {
        CreateRibbon();
    }

    private void CreateRibbon()
    {
        RibbonControl ribbon = ComponentManager.Ribbon;
        if (ribbon != null)
        {
            RibbonTab rtab = ribbon.FindTab(RibbonId);
            if (rtab != null)
            {
                ribbon.Tabs.Remove(rtab);
            }

            rtab = new RibbonTab();
            rtab.Title = RibbonTitle;
            rtab.Id = RibbonId;
            ribbon.Tabs.Add(rtab);
            AddContentToTab(rtab);
        }
    }

    private void AddContentToTab(RibbonTab rtab)
    {
        rtab.Panels.Add(AddPanelOne());
    }

    private static RibbonPanel AddPanelOne()
    {
        RibbonPanelSource rps = new RibbonPanelSource();
        rps.Title = "Addin Manager";
        RibbonPanel rp = new RibbonPanel();
        rp.Source = rps;
        RibbonButton rci = new RibbonButton();
        rci.Name = "Addin Manager";
        rps.DialogLauncher = rci;
        //create button1
        var addinAssembly = typeof(App).Assembly;
        RibbonButton btnPythonShell = new RibbonButton
        {
            Orientation = Orientation.Vertical,
            AllowInStatusBar = true,
            Size = RibbonItemSize.Large,
            Name = "Addin Manager Manual",
            ShowText = true,
            Text = "Addin Manager \n Manual",
            Description = "Start Write Addin Manager Manual",
            Image = GetEmbeddedPng(addinAssembly,
                "CadAddinManager.Resources.dev16x16.png"),
            LargeImage =
                GetEmbeddedPng(addinAssembly, "CadAddinManager.Resources.dev32x32.png"),
            CommandHandler = new RelayCommand(new AddInManagerManual().Execute)
        };
        rps.Items.Add(btnPythonShell);
        //create button2
        RibbonButton btnConfig = new RibbonButton
        {
            Orientation = Orientation.Vertical,
            AllowInStatusBar = true,
            Size = RibbonItemSize.Large,
            Name = "Addin Manager \n Faceless",
            ShowText = true,
            Text = "Addin Manager \n Faceless",
            Description = "Addin Manager Faceless",
            Image = GetEmbeddedPng(addinAssembly,
                "CadAddinManager.Resources.dev16x16.png"),
            LargeImage =
                GetEmbeddedPng(addinAssembly, "CadAddinManager.Resources.dev32x32.png"),
            CommandHandler = new RelayCommand(new AddInManagerFaceLess().Execute)
        };
        rps.Items.Add(btnConfig);
        //Create button 3
        RibbonButton btnLogcontrol = new RibbonButton
        {
            Orientation = Orientation.Vertical,
            AllowInStatusBar = true,
            Size = RibbonItemSize.Large,
            Name = "Show/Hide \n Dock Debug/Trace",
            ShowText = true,
            Text = "Show/Hide \n Dock Debug/Trace",
            Description = "Show/Hide Dock Debug/Trace",
            Image = GetEmbeddedPng(addinAssembly,
                "CadAddinManager.Resources.dev16x16.png"),
            LargeImage =
                GetEmbeddedPng(addinAssembly, "CadAddinManager.Resources.dev32x32.png"),
            CommandHandler = new RelayCommand(new DockPanelCommand().Execute)
        };
        rps.Items.Add(btnLogcontrol);
        return rp;
    }
    public static ImageSource GetEmbeddedPng(System.Reflection.Assembly app, string imageName)
    {
        var file = app.GetManifestResourceStream(imageName);
        var source = PngBitmapDecoder.Create(file, BitmapCreateOptions.None, BitmapCacheOption.None);
        return source.Frames[0];
    }
}