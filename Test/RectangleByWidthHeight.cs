using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;

namespace Test;

public class RectangleByWidthHeight
{
    [CommandMethod("RectWH")]
    public static void CreateRectangleByWidthHeight()
    {
        var doc = Application.DocumentManager.MdiActiveDocument;
        var db = doc.Database;
        var ed = doc.Editor;

        var baseRes = ed.GetPoint("\nBase (lower-left) point:");
        if (baseRes.Status != PromptStatus.OK) return;

        var wRes = ed.GetDistance("\nWidth:");
        if (wRes.Status != PromptStatus.OK) return;
        var hRes = ed.GetDistance("\nHeight:");
        if (hRes.Status != PromptStatus.OK) return;

        double w = wRes.Value;
        double h = hRes.Value;
        var p = baseRes.Value;

        using (var tr = db.TransactionManager.StartTransaction())
        {
            var bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
            var ms = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

            var pl = new Polyline { Closed = true };
            pl.AddVertexAt(0, new Point2d(p.X, p.Y), 0, 0, 0);
            pl.AddVertexAt(1, new Point2d(p.X + w, p.Y), 0, 0, 0);
            pl.AddVertexAt(2, new Point2d(p.X + w, p.Y + h), 0, 0, 0);
            pl.AddVertexAt(3, new Point2d(p.X, p.Y + h), 0, 0, 0);

            ms.AppendEntity(pl);
            tr.AddNewlyCreatedDBObject(pl, true);
            tr.Commit();
        }
    }
}
