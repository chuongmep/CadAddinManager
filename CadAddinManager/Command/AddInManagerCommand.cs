using System.Diagnostics;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.Runtime;
using CadAddinManager.Model;
using Exception = System.Exception;

namespace CadAddinManager.Command;


public class AddInManagerManual : ICadCommand
{
    
    [Autodesk.AutoCAD.Runtime.CommandMethod("AddInManagerManual",CommandFlags.Session)]
    public override void Execute()
    {
        Debug.Listeners.Clear();
        Trace.Listeners.Clear();
        CodeListener codeListener = new CodeListener();
        Debug.Listeners.Add(codeListener);
        AddinManagerBase.Instance.ExecuteCommand(false);
    }
}

public class AddInManagerFaceLess  : ICadCommand
{
    [Autodesk.AutoCAD.Runtime.CommandMethod("AddInManagerFaceLess",CommandFlags.Session)]
    public override void Execute()
    {
        AddinManagerBase.Instance.ExecuteCommand(true);
    }
}

