using System.Diagnostics;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.Runtime;
using CadAddinManager.Model;
using Exception = System.Exception;

namespace CadAddinManager.Command;


public class AddInManagerManual : ICadCommand
{
    
    [CommandMethod("AddInManagerManual",CommandFlags.Session)]
    public override void Execute()
    {
        Trace.Listeners.Clear();
        Trace.Listeners.Clear();
        CodeListener codeListener = new CodeListener();
        Trace.Listeners.Add(codeListener);
        AddinManagerBase.Instance.ExecuteCommand(false);
    }
}

public class AddInManagerFaceLess  : ICadCommand
{
    [Autodesk.AutoCAD.Runtime.CommandMethod("AddInManagerFaceLess",CommandFlags.Session)]
    public override void Execute()
    {
        try
        {
            AddinManagerBase.Instance.ExecuteCommand(true);
        }
        catch (Exception e)
        {
            MessageBox.Show(e.ToString());
            Trace.WriteLine(e.Message);
        }
    }
}

