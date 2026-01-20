using System;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;

namespace Test;

public class SimpleFractalTree
{
    [CommandMethod("FractalTree")]
    public static void CreateFractalTree()
    {
        var doc = Application.DocumentManager.MdiActiveDocument;
        var db = doc.Database;
        var ed = doc.Editor;

        var baseRes = ed.GetPoint("\nBase point:");
        if (baseRes.Status != PromptStatus.OK) return;
        var lenRes = ed.GetDistance("\nInitial branch length:");
        if (lenRes.Status != PromptStatus.OK) return;
        var depthRes = ed.GetInteger(new PromptIntegerOptions("\nDepth:") { LowerLimit = 1, UpperLimit = 10 });
        if (depthRes.Status != PromptStatus.OK) return;

        var p0 = baseRes.Value;
        double len = lenRes.Value;
        int depth = depthRes.Value;
        double angle = Math.PI / 2; // up

        using (var tr = db.TransactionManager.StartTransaction())
        {
            var bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
            var ms = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

            void DrawBranch(Point3d start, double length, double ang, int d)
            {
                if (d == 0 || length < 0.1) return;
                var end = new Point3d(start.X + length * Math.Cos(ang), start.Y + length * Math.Sin(ang), 0);
                var ln = new Line(start, end);
                ms.AppendEntity(ln);
                tr.AddNewlyCreatedDBObject(ln, true);
                DrawBranch(end, length * 0.7, ang + Math.PI / 6, d - 1);
                DrawBranch(end, length * 0.7, ang - Math.PI / 6, d - 1);
            }

            DrawBranch(p0, len, angle, depth);
            tr.Commit();
        }
    }
}
