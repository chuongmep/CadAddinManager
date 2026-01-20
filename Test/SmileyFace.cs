using System;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;

namespace Test;

public class SmileyFace
{
    [CommandMethod("Smiley")]
    public static void CreateSmiley()
    {
        var doc = Application.DocumentManager.MdiActiveDocument;
        var db = doc.Database;
        var ed = doc.Editor;

        var centerRes = ed.GetPoint("\nFace center:");
        if (centerRes.Status != PromptStatus.OK) return;
        var radiusRes = ed.GetDistance("\nFace radius:");
        if (radiusRes.Status != PromptStatus.OK) return;

        var c = centerRes.Value;
        double r = radiusRes.Value;
        double eyeOffsetX = r * 0.4;
        double eyeOffsetY = r * 0.35;
        double eyeR = r * 0.12;

        using (var tr = db.TransactionManager.StartTransaction())
        {
            var bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
            var ms = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

            // Face
            var face = new Circle(c, Vector3d.ZAxis, r);
            ms.AppendEntity(face);
            tr.AddNewlyCreatedDBObject(face, true);

            // Eyes
            var leftEye = new Circle(new Point3d(c.X - eyeOffsetX, c.Y + eyeOffsetY, 0), Vector3d.ZAxis, eyeR);
            var rightEye = new Circle(new Point3d(c.X + eyeOffsetX, c.Y + eyeOffsetY, 0), Vector3d.ZAxis, eyeR);
            ms.AppendEntity(leftEye);
            ms.AppendEntity(rightEye);
            tr.AddNewlyCreatedDBObject(leftEye, true);
            tr.AddNewlyCreatedDBObject(rightEye, true);

            // Smile arc
            double startAng = Math.PI * 0.1;
            double endAng = Math.PI * 0.9;
            var mouth = new Arc(c, r * 0.65, Math.PI + startAng, Math.PI + endAng);
            ms.AppendEntity(mouth);
            tr.AddNewlyCreatedDBObject(mouth, true);

            tr.Commit();
        }
    }
}
