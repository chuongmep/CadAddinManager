using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;

namespace Test;

public class TriangleFromThreePoints
{
    [CommandMethod("Triangle3Pts")]
    public static void CreateTriangle()
    {
        var doc = Application.DocumentManager.MdiActiveDocument;
        var db = doc.Database;
        var ed = doc.Editor;

        var p1 = ed.GetPoint("\nPick first point:");
        if (p1.Status != PromptStatus.OK) return;
        var p2 = ed.GetPoint("\nPick second point:");
        if (p2.Status != PromptStatus.OK) return;
        var p3 = ed.GetPoint("\nPick third point:");
        if (p3.Status != PromptStatus.OK) return;

        using (var tr = db.TransactionManager.StartTransaction())
        {
            var bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
            var ms = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

            var pl = new Polyline { Closed = true };
            pl.AddVertexAt(0, new Point2d(p1.Value.X, p1.Value.Y), 0, 0, 0);
            pl.AddVertexAt(1, new Point2d(p2.Value.X, p2.Value.Y), 0, 0, 0);
            pl.AddVertexAt(2, new Point2d(p3.Value.X, p3.Value.Y), 0, 0, 0);

            ms.AppendEntity(pl);
            tr.AddNewlyCreatedDBObject(pl, true);
            tr.Commit();
        }
    }
}
