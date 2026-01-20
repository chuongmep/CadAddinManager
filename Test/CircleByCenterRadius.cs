using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;

namespace Test;

public class CircleByCenterRadius
{
    [CommandMethod("CircleCR")]
    public static void CreateCircleCR()
    {
        var doc = Application.DocumentManager.MdiActiveDocument;
        var db = doc.Database;
        var ed = doc.Editor;

        var cRes = ed.GetPoint("\nCenter point:");
        if (cRes.Status != PromptStatus.OK) return;
        var rRes = ed.GetDistance("\nRadius:");
        if (rRes.Status != PromptStatus.OK) return;

        using (var tr = db.TransactionManager.StartTransaction())
        {
            var bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
            var ms = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

            var circle = new Circle(cRes.Value, Vector3d.ZAxis, rRes.Value);
            ms.AppendEntity(circle);
            tr.AddNewlyCreatedDBObject(circle, true);
            tr.Commit();
        }
    }
}
