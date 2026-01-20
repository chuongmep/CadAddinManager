using System;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;

namespace Test;

public class FlowerPetals
{
    [CommandMethod("Flower")]
    public static void CreateFlower()
    {
        var doc = Application.DocumentManager.MdiActiveDocument;
        var db = doc.Database;
        var ed = doc.Editor;

        var centerRes = ed.GetPoint("\nCenter:");
        if (centerRes.Status != PromptStatus.OK) return;
        var radiusRes = ed.GetDistance("\nPetal radius:");
        if (radiusRes.Status != PromptStatus.OK) return;
        var countRes = ed.GetInteger(new PromptIntegerOptions("\nPetals:") { LowerLimit = 3, UpperLimit = 64 });
        if (countRes.Status != PromptStatus.OK) return;

        var c = centerRes.Value;
        double r = radiusRes.Value;
        int n = countRes.Value;
        double minorRatio = 0.5;

        using (var tr = db.TransactionManager.StartTransaction())
        {
            var bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
            var ms = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

            for (int i = 0; i < n; i++)
            {
                double ang = (2 * Math.PI / n) * i;
                var dir = new Vector3d(Math.Cos(ang), Math.Sin(ang), 0) * r;
                var ellipse = new Ellipse(c, Vector3d.ZAxis, dir, minorRatio, 0, 2 * Math.PI);
                ms.AppendEntity(ellipse);
                tr.AddNewlyCreatedDBObject(ellipse, true);
            }

            // center circle
            var centerCircle = new Circle(c, Vector3d.ZAxis, r * 0.2);
            ms.AppendEntity(centerCircle);
            tr.AddNewlyCreatedDBObject(centerCircle, true);

            tr.Commit();
        }
    }
}
