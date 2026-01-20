using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;

namespace Test;

public class OffsetPolylineSample
{
    [CommandMethod("OffsetPL")]
    public static void OffsetSelectedPolyline()
    {
        var doc = Application.DocumentManager.MdiActiveDocument;
        var db = doc.Database;
        var ed = doc.Editor;

        var peo = new PromptEntityOptions("\nSelect Polyline to offset:");
        peo.SetRejectMessage("\nEntity is not a Polyline.");
        peo.AddAllowedClass(typeof(Polyline), exactMatch: true);
        var per = ed.GetEntity(peo);
        if (per.Status != PromptStatus.OK) return;

        var distRes = ed.GetDistance("\nOffset distance:");
        if (distRes.Status != PromptStatus.OK) return;
        double d = distRes.Value;

        using (var tr = db.TransactionManager.StartTransaction())
        {
            var pl = (Polyline)tr.GetObject(per.ObjectId, OpenMode.ForRead);
            var bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
            var ms = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

            var offs = pl.GetOffsetCurves(d);
            foreach (DBObject dbo in offs)
            {
                if (dbo is Entity ent)
                {
                    ms.AppendEntity(ent);
                    tr.AddNewlyCreatedDBObject(ent, true);
                }
            }
            tr.Commit();
        }
    }
}
