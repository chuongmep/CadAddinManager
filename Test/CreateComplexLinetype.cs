using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;


public class CreateComplexLinetype
{
    // code modified from the link 
//http://through-the-interface.typepad.com/through_the_interface/2008/01/creating-a-comp.html
[CommandMethod("CCL")]
public void CreateComplexLinetypeTest()
{
    Document doc =
        Application.DocumentManager.MdiActiveDocument;
    Database db = doc.Database;
    Editor ed = doc.Editor;
    Transaction tr =
        db.TransactionManager.StartTransaction();
    using (tr)
    {
        TextStyleTable tt =
            (TextStyleTable)tr.GetObject(
            db.TextStyleTableId,
            OpenMode.ForRead
            );
        LinetypeTable lt =
            (LinetypeTable)tr.GetObject(
            db.LinetypeTableId,
            OpenMode.ForWrite
            );
        LinetypeTableRecord ltr =
            new LinetypeTableRecord();

        ltr.Name = "COLD_WATER_SUPPLY";
        ltr.AsciiDescription =
            "Cold water supply ---- CW ---- CW ---- CW ----";
        ltr.PatternLength = 0.9;
        ltr.NumDashes = 3;
        // Dash #1
        ltr.SetDashLengthAt(0, 0.5);
        // Dash #2
        ltr.SetDashLengthAt(1,-0.2);
        ltr.SetShapeStyleAt(1, tt["Standard"]);
        ltr.SetShapeNumberAt(1, 0);
        ltr.SetShapeScaleAt(1, 0.1);
        ltr.SetTextAt(1, "CW");
        ltr.SetShapeRotationAt(1, 0);
        ltr.SetShapeOffsetAt(1, new Vector2d(0, -0.05));
        // Dash #3
        ltr.SetDashLengthAt(2, -0.2);

        // Add the new linetype to the linetype table
        ObjectId ltId = lt.Add(ltr);                
        tr.AddNewlyCreatedDBObject(ltr, true);
        // Create a test line with this linetype
        BlockTable bt =
            (BlockTable)tr.GetObject(
            db.BlockTableId,
            OpenMode.ForRead
            );
        BlockTableRecord btr =
            (BlockTableRecord)tr.GetObject(
            bt[BlockTableRecord.ModelSpace],
    OpenMode.ForWrite
    );

        using (Polyline acPoly = new Polyline())
        {
            acPoly.SetDatabaseDefaults(db);
            acPoly.AddVertexAt(0, new Point2d(0, 0), 0, 0, 0);
            acPoly.AddVertexAt(1, new Point2d(0, 2), 0, 0, 0);
            acPoly.AddVertexAt(2, new Point2d(2, 2), 0, 0, 0);
            acPoly.AddVertexAt(3, new Point2d(2, 0), 0, 0, 0);
            acPoly.Closed = true;
            btr.AppendEntity(acPoly);
            tr.AddNewlyCreatedDBObject(acPoly, false);
            Polyline2d poly2 = acPoly.ConvertTo(true);
            poly2.LinetypeGenerationOn = false;
            poly2.LinetypeId = ltId;
            tr.AddNewlyCreatedDBObject(poly2, true);
        }
        tr.Commit();
    }
}
}