using System;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;

namespace Test;

public class StarPolygon
{
    [CommandMethod("StarPoly")]
    public static void CreateStarPolygon()
    {
        var doc = Application.DocumentManager.MdiActiveDocument;
        var db = doc.Database;
        var ed = doc.Editor;

        var centerRes = ed.GetPoint("\nCenter:");
        if (centerRes.Status != PromptStatus.OK) return;
        var radiusRes = ed.GetDistance("\nOuter radius:");
        if (radiusRes.Status != PromptStatus.OK) return;
        var sidesOpts = new PromptIntegerOptions("\nNumber of points:") { LowerLimit = 3, UpperLimit = 64 };
        var sidesRes = ed.GetInteger(sidesOpts);
        if (sidesRes.Status != PromptStatus.OK) return;

        int n = sidesRes.Value;
        double rOuter = radiusRes.Value;
        double rInner = rOuter * 0.5;
        var c = centerRes.Value;

        using (var tr = db.TransactionManager.StartTransaction())
        {
            var bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
            var ms = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

            var pl = new Polyline { Closed = true };
            for (int i = 0; i < n * 2; i++)
            {
                double ang = (Math.PI / n) * i;
                double r = (i % 2 == 0) ? rOuter : rInner;
                double x = c.X + r * Math.Cos(ang);
                double y = c.Y + r * Math.Sin(ang);
                pl.AddVertexAt(i, new Point2d(x, y), 0, 0, 0);
            }
            ms.AppendEntity(pl);
            tr.AddNewlyCreatedDBObject(pl, true);
            tr.Commit();
        }
    }
}
