using System;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;

namespace Test;

public class RandomTriangles
{
    [CommandMethod("RandTriangles")]
    public static void CreateRandomTriangles()
    {
        var doc = Application.DocumentManager.MdiActiveDocument;
        var db = doc.Database;
        var ed = doc.Editor;

        var p1Res = ed.GetPoint("\nPick first corner:");
        if (p1Res.Status != PromptStatus.OK) return;
        var p2Res = ed.GetCorner(new PromptCornerOptions("\nPick opposite corner:", p1Res.Value));
        if (p2Res.Status != PromptStatus.OK) return;
        var countRes = ed.GetInteger(new PromptIntegerOptions("\nHow many triangles:") { LowerLimit = 1, UpperLimit = 200 });
        if (countRes.Status != PromptStatus.OK) return;

        var minX = Math.Min(p1Res.Value.X, p2Res.Value.X);
        var maxX = Math.Max(p1Res.Value.X, p2Res.Value.X);
        var minY = Math.Min(p1Res.Value.Y, p2Res.Value.Y);
        var maxY = Math.Max(p1Res.Value.Y, p2Res.Value.Y);
        var rand = new Random();

        using (var tr = db.TransactionManager.StartTransaction())
        {
            var bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
            var ms = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

            for (int i = 0; i < countRes.Value; i++)
            {
                Point2d pA = new Point2d(minX + rand.NextDouble() * (maxX - minX), minY + rand.NextDouble() * (maxY - minY));
                Point2d pB = new Point2d(minX + rand.NextDouble() * (maxX - minX), minY + rand.NextDouble() * (maxY - minY));
                Point2d pC = new Point2d(minX + rand.NextDouble() * (maxX - minX), minY + rand.NextDouble() * (maxY - minY));
                var pl = new Polyline { Closed = true };
                pl.AddVertexAt(0, pA, 0, 0, 0);
                pl.AddVertexAt(1, pB, 0, 0, 0);
                pl.AddVertexAt(2, pC, 0, 0, 0);
                ms.AppendEntity(pl);
                tr.AddNewlyCreatedDBObject(pl, true);
            }
            tr.Commit();
        }
    }
}
