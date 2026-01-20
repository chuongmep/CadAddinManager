using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;

namespace Test;

public class SetCurrentLayerSimple
{
    [CommandMethod("SetCurLayer")]
    public static void SetCurrentLayer()
    {
        var doc = Application.DocumentManager.MdiActiveDocument;
        var db = doc.Database;
        var ed = doc.Editor;

        var sRes = ed.GetString("\nLayer name to set current:");
        if (sRes.Status != Autodesk.AutoCAD.EditorInput.PromptStatus.OK) return;
        var name = sRes.StringResult;

        using (var tr = db.TransactionManager.StartTransaction())
        {
            var lt = (LayerTable)tr.GetObject(db.LayerTableId, OpenMode.ForRead);
            if (!lt.Has(name))
            {
                ed.WriteMessage($"\nLayer '{name}' not found.");
                return;
            }
            db.Clayer = lt[name];
            tr.Commit();
            ed.WriteMessage($"\nCurrent layer set to '{name}'.");
        }
    }
}
