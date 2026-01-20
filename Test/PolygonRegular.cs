using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;

namespace Test;

public class PolygonRegular
{
    [CommandMethod("PolygonN")]
    public static void CreateRegularPolygon()
    {
        var doc = Application.DocumentManager.MdiActiveDocument;
        var db = doc.Database;
        var ed = doc.Editor;

        var centerRes = ed.GetPoint("\nCenter point:");
        if (centerRes.Status != PromptStatus.OK) return;

        var radiusRes = ed.GetDistance("\nRadius:");
        if (radiusRes.Status != PromptStatus.OK) return;

        var sidesOpts = new PromptIntegerOptions("\nNumber of sides:")
        {
            AllowNone = false,
            LowerLimit = 3,
            UpperLimit = 128
        };
        var sidesRes = ed.GetInteger(sidesOpts);
        if (sidesRes.Status != PromptStatus.OK) return;

        int n = sidesRes.Value;
        double r = radiusRes.Value;
        var c = centerRes.Value;

        using (var tr = db.TransactionManager.StartTransaction())
        {
            var bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
            var ms = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

            var pl = new Polyline { Closed = true };
            for (int i = 0; i < n; i++)
            {
                double ang = (2.0 * System.Math.PI / n) * i;
                double x = c.X + r * System.Math.Cos(ang);
                double y = c.Y + r * System.Math.Sin(ang);
                pl.AddVertexAt(i, new Point2d(x, y), 0, 0, 0);
            }
            ms.AppendEntity(pl);
            tr.AddNewlyCreatedDBObject(pl, true);
            tr.Commit();
        }
    }
}
