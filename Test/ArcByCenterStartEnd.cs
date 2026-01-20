using System;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;

namespace Test;

public class ArcByCenterStartEnd
{
    [CommandMethod("ArcCSE")]
    public static void CreateArcCSE()
    {
        var doc = Application.DocumentManager.MdiActiveDocument;
        var db = doc.Database;
        var ed = doc.Editor;

        var cRes = ed.GetPoint("\nCenter point:");
        if (cRes.Status != PromptStatus.OK) return;
        var sRes = ed.GetPoint("\nStart point:");
        if (sRes.Status != PromptStatus.OK) return;
        var eRes = ed.GetPoint("\nEnd point:");
        if (eRes.Status != PromptStatus.OK) return;

        var c = cRes.Value;
        var s = sRes.Value;
        var e = eRes.Value;

        var vS = new Vector2d(s.X - c.X, s.Y - c.Y);
        var vE = new Vector2d(e.X - c.X, e.Y - c.Y);
        double startAng = Math.Atan2(vS.Y, vS.X);
        double endAng = Math.Atan2(vE.Y, vE.X);
        double r = c.DistanceTo(s);

        using (var tr = db.TransactionManager.StartTransaction())
        {
            var bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
            var ms = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

            var arc = new Arc(c, r, startAng, endAng);
            ms.AppendEntity(arc);
            tr.AddNewlyCreatedDBObject(arc, true);
            tr.Commit();
        }
    }
}
