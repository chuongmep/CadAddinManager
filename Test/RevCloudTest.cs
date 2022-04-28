using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;

namespace Test;

public class RevCloudTest
{
    //Test Case Null Command String
    [CommandMethod("RC")]
    public void RevCloud()

    {
        var doc = Application.DocumentManager.MdiActiveDocument;
        if (doc == null) return;
        var db = doc.Database;

        var ed = doc.Editor;
        using (var tr = db.TransactionManager.StartTransaction())
        {
            var ms = tr.GetObject(SymbolUtilityServices.GetBlockModelSpaceId(db),
                OpenMode.ForWrite) as BlockTableRecord;
            if (ms != null)
            {
                // Create our polyline boundary
                var pl = new Polyline();

                pl.AddVertexAt(0, new Point2d(0, 0), 0, 0, 0);

                pl.AddVertexAt(1, new Point2d(10, 0), 0, 0, 0);

                pl.AddVertexAt(2, new Point2d(10, 10), 0, 0, 0);

                pl.AddVertexAt(3, new Point2d(0, 10), 0, 0, 0);

                pl.Closed = true;
                // Add it to the drawing
                var id = ms.AppendEntity(pl);

                tr.AddNewlyCreatedDBObject(pl, true);
                // Create a SelectionSet and use it to call the REVCLOUD command
                var ss = SelectionSet.FromObjectIds(new ObjectId[] {id});

                ed.Command("_.REVCLOUD", "", ss, "");
            }

            tr.Commit();
        }
    }
}