using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;

namespace Test;

public class LineBetweenTwoPoints
{
    [CommandMethod("Line2Pts")]
    public static void CreateLine2Pts()
    {
        var doc = Application.DocumentManager.MdiActiveDocument;
        var db = doc.Database;
        var ed = doc.Editor;

        var p1 = ed.GetPoint("\nFirst point:");
        if (p1.Status != PromptStatus.OK) return;
        var p2 = ed.GetPoint("\nSecond point:");
        if (p2.Status != PromptStatus.OK) return;

        using (var tr = db.TransactionManager.StartTransaction())
        {
            var bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
            var ms = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

            var ln = new Line(p1.Value, p2.Value);
            ms.AppendEntity(ln);
            tr.AddNewlyCreatedDBObject(ln, true);
            tr.Commit();
        }
    }
}
