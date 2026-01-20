using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;

namespace Test;

public class ConcentricCircles
{
    [CommandMethod("Concentric")]
    public static void CreateConcentricCircles()
    {
        var doc = Application.DocumentManager.MdiActiveDocument;
        var db = doc.Database;
        var ed = doc.Editor;

        var centerRes = ed.GetPoint("\nCenter:");
        if (centerRes.Status != PromptStatus.OK) return;
        var firstRes = ed.GetDistance("\nFirst radius:");
        if (firstRes.Status != PromptStatus.OK) return;
        var stepRes = ed.GetDistance("\nRadius step:");
        if (stepRes.Status != PromptStatus.OK) return;
        var countRes = ed.GetInteger(new PromptIntegerOptions("\nHow many:") { LowerLimit = 1, UpperLimit = 100 });
        if (countRes.Status != PromptStatus.OK) return;

        var c = centerRes.Value;
        double r = firstRes.Value;
        double step = stepRes.Value;
        int count = countRes.Value;

        using (var tr = db.TransactionManager.StartTransaction())
        {
            var bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
            var ms = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

            for (int i = 0; i < count; i++)
            {
                var circle = new Circle(c, Vector3d.ZAxis, r + i * step);
                ms.AppendEntity(circle);
                tr.AddNewlyCreatedDBObject(circle, true);
            }
            tr.Commit();
        }
    }
}
