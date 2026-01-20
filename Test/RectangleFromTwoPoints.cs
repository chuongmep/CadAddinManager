using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;

namespace Test;

public class RectangleFromTwoPoints
{
    [CommandMethod("Rect2Pts")]
    public static void CreateRectangleFromTwoCorners()
    {
        var doc = Application.DocumentManager.MdiActiveDocument;
        var db = doc.Database;
        var ed = doc.Editor;

        var p1Res = ed.GetPoint("\nPick first corner:");
        if (p1Res.Status != PromptStatus.OK) return;
        var p2Res = ed.GetCorner(new PromptCornerOptions("\nPick opposite corner:", p1Res.Value));
        if (p2Res.Status != PromptStatus.OK) return;

        var p1 = p1Res.Value;
        var p2 = p2Res.Value;

        // Compute other corners (axis-aligned)
        var p3 = new Point3d(p1.X, p2.Y, p1.Z);
        var p4 = new Point3d(p2.X, p1.Y, p1.Z);

        using (var tr = db.TransactionManager.StartTransaction())
        {
            var bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
            var ms = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

            var pl = new Polyline { Closed = true };
            pl.AddVertexAt(0, new Point2d(p1.X, p1.Y), 0, 0, 0);
            pl.AddVertexAt(1, new Point2d(p3.X, p3.Y), 0, 0, 0);
            pl.AddVertexAt(2, new Point2d(p2.X, p2.Y), 0, 0, 0);
            pl.AddVertexAt(3, new Point2d(p4.X, p4.Y), 0, 0, 0);

            ms.AppendEntity(pl);
            tr.AddNewlyCreatedDBObject(pl, true);
            tr.Commit();
        }
    }
}
