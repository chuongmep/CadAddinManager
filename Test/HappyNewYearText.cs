using System;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;

namespace Test;

public class HappyNewYearText
{
    [CommandMethod("HappyNewYear")]
    public static void CreateHappyNewYear()
    {
        var doc = Application.DocumentManager.MdiActiveDocument;
        var db = doc.Database;
        var ed = doc.Editor;

        // Pick insertion point
        var insRes = ed.GetPoint("\nPick center point for text:");
        if (insRes.Status != PromptStatus.OK) return;

        // Ask for text height
        var hOpts = new PromptDoubleOptions("\nText height:") { AllowNegative = false, AllowZero = false, DefaultValue = 20.0 };
        var hRes = ed.GetDouble(hOpts);
        if (hRes.Status != PromptStatus.OK) return;
        double textHeight = hRes.Value;

        // Ask for confetti count
        var confOpts = new PromptIntegerOptions("\nConfetti count:") { LowerLimit = 0, UpperLimit = 500, DefaultValue = 60 };
        var confRes = ed.GetInteger(confOpts);
        if (confRes.Status != PromptStatus.OK) return;
        int confettiCount = confRes.Value;

        using (var tr = db.TransactionManager.StartTransaction())
        {
            // Ensure celebration layer exists
            var lt = (LayerTable)tr.GetObject(db.LayerTableId, OpenMode.ForWrite);
            string layerName = "Celebration";
            if (!lt.Has(layerName))
            {
                var ltr = new LayerTableRecord { Name = layerName, Color = Color.FromColorIndex(ColorMethod.ByAci, 6) }; // magenta
                lt.Add(ltr);
                tr.AddNewlyCreatedDBObject(ltr, true);
            }
            db.Clayer = lt[layerName];

            // Ensure text style exists (tries Arial Black; falls back to default)
            var tst = (TextStyleTable)tr.GetObject(db.TextStyleTableId, OpenMode.ForWrite);
            string styleName = "HNY_Style";
            ObjectId styleId;
            if (!tst.Has(styleName))
            {
                var tsr = new TextStyleTableRecord { Name = styleName, FileName = "Arial Black" };
                tst.Add(tsr);
                tr.AddNewlyCreatedDBObject(tsr, true);
                styleId = tsr.ObjectId;
            }
            else
            {
                styleId = tst[styleName];
            }

            // Create MText with formatted content
            var mt = new MText
            {
                Location = insRes.Value,
                TextHeight = textHeight,
                Attachment = AttachmentPoint.MiddleCenter,
                TextStyleId = styleId,
                Color = Color.FromColorIndex(ColorMethod.ByAci, 1) // red
            };
            // Use MText formatting codes: font bold and color changes
            mt.Contents = "\\fArial Black|b1|i0;\\C1;Happy New Year!\\P\\C3;2026"; // red HNY, green year

            var bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
            var ms = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

            ms.AppendEntity(mt);
            tr.AddNewlyCreatedDBObject(mt, true);

            // Try to compute extents to place confetti around
            try
            {
                var ext = mt.GeometricExtents;
                var min = ext.MinPoint;
                var max = ext.MaxPoint;
                // Expand bounds for confetti area
                double margin = textHeight * 0.8;
                min = new Point3d(min.X - margin, min.Y - margin, 0);
                max = new Point3d(max.X + margin, max.Y + margin, 0);

                var rand = new Random();
                // Create confetti circles with random colors
                for (int i = 0; i < confettiCount; i++)
                {
                    double x = min.X + rand.NextDouble() * (max.X - min.X);
                    double y = min.Y + rand.NextDouble() * (max.Y - min.Y);
                    double r = Math.Max(0.2, textHeight * (0.03 + 0.05 * rand.NextDouble()));
                    var circ = new Circle(new Point3d(x, y, 0), Vector3d.ZAxis, r);
                    short aci = (short)new int[] { 1, 2, 3, 4, 5, 6, 140, 30 }[rand.Next(8)];
                    circ.Color = Color.FromColorIndex(ColorMethod.ByAci, aci);
                    ms.AppendEntity(circ);
                    tr.AddNewlyCreatedDBObject(circ, true);
                }
            }
            catch
            {
                // If extents fail (rare), skip confetti gracefully
                ed.WriteMessage("\nNote: Could not compute text extents for confetti placement.");
            }

            tr.Commit();
        }
    }
}
