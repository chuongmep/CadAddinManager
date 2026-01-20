using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;

namespace Test;

public class MirrorSelection
{
    [CommandMethod("MirrorSel")]
    public static void MirrorSelectedEntities()
    {
        var doc = Application.DocumentManager.MdiActiveDocument;
        var db = doc.Database;
        var ed = doc.Editor;

        var selRes = ed.GetSelection();
        if (selRes.Status != PromptStatus.OK) return;

        var p1Res = ed.GetPoint("\nMirror line start:");
        if (p1Res.Status != PromptStatus.OK) return;
        var p2Res = ed.GetPoint("\nMirror line end:");
        if (p2Res.Status != PromptStatus.OK) return;

        var line = new Line3d(p1Res.Value, p2Res.Value);
        var mirror = Matrix3d.Mirroring(line);

        using (var tr = db.TransactionManager.StartTransaction())
        {
            var bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
            var ms = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

            foreach (SelectedObject so in selRes.Value)
            {
                if (so.ObjectId.IsNull) continue;
                var ent = tr.GetObject(so.ObjectId, OpenMode.ForRead) as Entity;
                if (ent == null) continue;
                var clone = ent.Clone() as Entity;
                if (clone == null) continue;
                clone.TransformBy(mirror);
                ms.AppendEntity(clone);
                tr.AddNewlyCreatedDBObject(clone, true);
            }
            tr.Commit();
        }
    }
}
