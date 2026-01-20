using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;

namespace Test;

public class GridOfRectangles
{
    [CommandMethod("GridRects")]
    public static void CreateGridOfRectangles()
    {
        var doc = Application.DocumentManager.MdiActiveDocument;
        var db = doc.Database;
        var ed = doc.Editor;

        var baseRes = ed.GetPoint("\nLower-left base:");
        if (baseRes.Status != PromptStatus.OK) return;
        var rowsRes = ed.GetInteger(new PromptIntegerOptions("\nRows:") { LowerLimit = 1, UpperLimit = 100 });
        if (rowsRes.Status != PromptStatus.OK) return;
        var colsRes = ed.GetInteger(new PromptIntegerOptions("\nColumns:") { LowerLimit = 1, UpperLimit = 100 });
        if (colsRes.Status != PromptStatus.OK) return;
        var wRes = ed.GetDistance("\nCell width:");
        if (wRes.Status != PromptStatus.OK) return;
        var hRes = ed.GetDistance("\nCell height:");
        if (hRes.Status != PromptStatus.OK) return;

        var p = baseRes.Value;
        double w = wRes.Value;
        double h = hRes.Value;
        int rows = rowsRes.Value;
        int cols = colsRes.Value;

        using (var tr = db.TransactionManager.StartTransaction())
        {
            var bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
            var ms = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    double x = p.X + c * w;
                    double y = p.Y + r * h;
                    var pl = new Polyline { Closed = true };
                    pl.AddVertexAt(0, new Point2d(x, y), 0, 0, 0);
                    pl.AddVertexAt(1, new Point2d(x + w, y), 0, 0, 0);
                    pl.AddVertexAt(2, new Point2d(x + w, y + h), 0, 0, 0);
                    pl.AddVertexAt(3, new Point2d(x, y + h), 0, 0, 0);
                    ms.AppendEntity(pl);
                    tr.AddNewlyCreatedDBObject(pl, true);
                }
            }
            tr.Commit();
        }
    }
}
