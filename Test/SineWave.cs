using System;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;

namespace Test;

public class SineWave
{
    [CommandMethod("SineWave")]
    public static void CreateSineWave()
    {
        var doc = Application.DocumentManager.MdiActiveDocument;
        var db = doc.Database;
        var ed = doc.Editor;

        var startRes = ed.GetPoint("\nStart point:");
        if (startRes.Status != PromptStatus.OK) return;
        var lengthRes = ed.GetDistance("\nLength (X):");
        if (lengthRes.Status != PromptStatus.OK) return;
        var ampRes = ed.GetDistance("\nAmplitude:");
        if (ampRes.Status != PromptStatus.OK) return;
        var cyclesRes = ed.GetInteger(new PromptIntegerOptions("\nCycles:") { LowerLimit = 1, UpperLimit = 100 });
        if (cyclesRes.Status != PromptStatus.OK) return;

        var p0 = startRes.Value;
        double L = lengthRes.Value;
        double A = ampRes.Value;
        int C = cyclesRes.Value;
        int samples = 200;

        using (var tr = db.TransactionManager.StartTransaction())
        {
            var bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
            var ms = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);
            var pl = new Polyline();

            for (int i = 0; i <= samples; i++)
            {
                double x = p0.X + (L * i / samples);
                double y = p0.Y + A * Math.Sin(2 * Math.PI * C * i / samples);
                pl.AddVertexAt(i, new Point2d(x, y), 0, 0, 0);
            }
            ms.AppendEntity(pl);
            tr.AddNewlyCreatedDBObject(pl, true);
            tr.Commit();
        }
    }
}
