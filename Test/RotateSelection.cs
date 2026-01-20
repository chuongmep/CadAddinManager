using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;

namespace Test;

public class RotateSelection
{
    [CommandMethod("RotateSel")]
    public static void RotateSelectedEntities()
    {
        var doc = Application.DocumentManager.MdiActiveDocument;
        var db = doc.Database;
        var ed = doc.Editor;

        var selRes = ed.GetSelection();
        if (selRes.Status != PromptStatus.OK) return;

        var baseRes = ed.GetPoint("\nBase point:");
        if (baseRes.Status != PromptStatus.OK) return;

        var angRes = ed.GetAngle(baseRes.Value.ToString());
        if (angRes.Status != PromptStatus.OK) return;
        double angleRad = angRes.Value; // already radians

        var rot = Matrix3d.Rotation(angleRad, Vector3d.ZAxis, baseRes.Value);

        using (var tr = db.TransactionManager.StartTransaction())
        {
            var bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
            var ms = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

            foreach (SelectedObject so in selRes.Value)
            {
                var ent = tr.GetObject(so.ObjectId, OpenMode.ForRead) as Entity;
                if (ent == null) continue;
                var clone = ent.Clone() as Entity;
                if (clone == null) continue;
                clone.TransformBy(rot);
                ms.AppendEntity(clone);
                tr.AddNewlyCreatedDBObject(clone, true);
            }
            tr.Commit();
        }
    }
}
