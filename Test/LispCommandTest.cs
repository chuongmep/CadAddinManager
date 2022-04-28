using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;

namespace Test;

public class LispCommand
{
    [CommandMethod("TestLispFunction", CommandFlags.Modal)]
    public void Test()
    {
        ResultBuffer args = new ResultBuffer(
            new TypedValue((int)LispDataType.Text, "c:hello"));
        ResultBuffer result = Application.Invoke(args);
        Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
        ed.WriteMessage(result.ToString());
    }
}