using System;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;

namespace Test;

public class SpiralPolyline
{
    [CommandMethod("SpiralPL")]
    public static void CreateSpiral()
    {
        var doc = Application.DocumentManager.MdiActiveDocument;
        var db = doc.Database;
        var ed = doc.Editor;

        var centerRes = ed.GetPoint("\nCenter:");
        if (centerRes.Status != PromptStatus.OK) return;
        var turnsOpts = new PromptIntegerOptions("\nTurns:") { LowerLimit = 1, UpperLimit = 50 };
        var turnsRes = ed.GetInteger(turnsOpts);
        if (turnsRes.Status != PromptStatus.OK) return;
        var stepRes = ed.GetDistance("\nRadius step per turn:");
        if (stepRes.Status != PromptStatus.OK) return;

        var c = centerRes.Value;
        int turns = turnsRes.Value;
        double step = stepRes.Value;
        int samples = turns * 60;

        using (var tr = db.TransactionManager.StartTransaction())
        {
            var bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
            var ms = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);
            var pl = new Polyline();

            for (int i = 0; i <= samples; i++)
            {
                double t = (2 * Math.PI) * (double)i / 60.0;
                double r = (step / (2 * Math.PI)) * t;
                double x = c.X + r * Math.Cos(t);
                double y = c.Y + r * Math.Sin(t);
                pl.AddVertexAt(i, new Point2d(x, y), 0, 0, 0);
            }

            ms.AppendEntity(pl);
            tr.AddNewlyCreatedDBObject(pl, true);
            tr.Commit();
        }
    }
}
