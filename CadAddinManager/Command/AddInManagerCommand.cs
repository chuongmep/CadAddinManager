using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.Runtime;
using Exception = System.Exception;

namespace CadAddinManager.Command;


public class AddInManagerManual : ICadCommand
{
    
    [Autodesk.AutoCAD.Runtime.CommandMethod("AddInManagerManual",CommandFlags.Session)]
    public override void Execute()
    {
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

