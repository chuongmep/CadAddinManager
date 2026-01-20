using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;

namespace Test;

public class MoveSelection
{
    [CommandMethod("MoveSel")]
    public static void MoveSelectedEntities()
    {
        var doc = Application.DocumentManager.MdiActiveDocument;
        var db = doc.Database;
        var ed = doc.Editor;

        var sel = ed.GetSelection();
        if (sel.Status != PromptStatus.OK) return;

        var fromRes = ed.GetPoint("\nBase point:");
        if (fromRes.Status != PromptStatus.OK) return;
        var toRes = ed.GetPoint("\nSecond point:");
        if (toRes.Status != PromptStatus.OK) return;

        var disp = toRes.Value - fromRes.Value;
        var mat = Matrix3d.Displacement(disp);

        using (var tr = db.TransactionManager.StartTransaction())
        {
            var bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
            var ms = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

            foreach (SelectedObject so in sel.Value)
            {
                var ent = tr.GetObject(so.ObjectId, OpenMode.ForWrite) as Entity;
                if (ent == null) continue;
                ent.TransformBy(mat);
            }
            tr.Commit();
        }
    }
}
