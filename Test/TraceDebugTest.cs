using Autodesk.AutoCAD.Runtime;

namespace Test;

public class TraceDebugTest
{
    [CommandMethod("HelloTrace")]
    public void HelloTrace()
    {
        try
        {
            throw new Exception(ErrorStatus.BadLinetypeScale);
        }
        catch (Exception ex)
        {
            var editor = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument.Editor;
            editor.WriteMessage(ex.ToString());
            editor.WriteMessage(ex.StackTrace);
        }
    }
}