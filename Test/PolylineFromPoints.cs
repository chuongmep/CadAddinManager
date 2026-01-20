using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using System.Collections.Generic;

namespace Test;

public class PolylineFromPoints
{
    [CommandMethod("PolylinePts")]
    public static void CreatePolylinePts()
    {
        var doc = Application.DocumentManager.MdiActiveDocument;
        var db = doc.Database;
        var ed = doc.Editor;

        var pts = new List<Point3d>();
        while (true)
        {
            var pRes = ed.GetPoint("\nPick point or press Enter to finish:");
            if (pRes.Status == PromptStatus.OK)
            {
                pts.Add(pRes.Value);
            }
            else
            {
                break;
            }
        }

        if (pts.Count < 2)
        {
            ed.WriteMessage("\nNeed at least 2 points.");
            return;
        }

        using (var tr = db.TransactionManager.StartTransaction())
        {
            var bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
            var ms = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

            var pl = new Polyline { Closed = false };
            for (int i = 0; i < pts.Count; i++)
            {
                pl.AddVertexAt(i, new Point2d(pts[i].X, pts[i].Y), 0, 0, 0);
            }
            ms.AppendEntity(pl);
            tr.AddNewlyCreatedDBObject(pl, true);
            tr.Commit();
        }
    }
}
