using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;


public class CreateBlockTest
{
    //Test Case Command Execute String
    [CommandMethod("CB")]
    public void CreateBlock()

    {
        var doc = Application.DocumentManager.MdiActiveDocument;

        if (doc == null)

            return;


        var db = doc.Database;

        var ed = doc.Editor;


        var ids = new ObjectIdCollection();


        // Create the geometry for our block using the AutoCAD .NET API


        using (var tr = db.TransactionManager.StartTransaction())

        {
            var ms =
                tr.GetObject(
                    SymbolUtilityServices.GetBlockModelSpaceId(db),
                    OpenMode.ForWrite
                ) as BlockTableRecord;


            if (ms != null)

            {
                // We'll have two diagonal lines with a circle


                var ln1 = new Line(new Point3d(0, 0, 0), new Point3d(10, 10, 0));

                var ln2 = new Line(new Point3d(0, 10, 0), new Point3d(10, 0, 0));

                var c = new Circle(new Point3d(5, 5, 0), Vector3d.ZAxis, 5);


                ids.Add(ms.AppendEntity(ln1));

                ids.Add(ms.AppendEntity(ln2));

                ids.Add(ms.AppendEntity(c));


                tr.AddNewlyCreatedDBObject(ln1, true);

                tr.AddNewlyCreatedDBObject(ln2, true);

                tr.AddNewlyCreatedDBObject(c, true);
            }


            tr.Commit();


            // Add the ObjectIds of our geometry to a SelectionSet


            var ida = new ObjectId[ids.Count];

            ids.CopyTo(ida, 0);

            var ss = SelectionSet.FromObjectIds(ida);


            // Create the block and insert it using standard commands


            ed.Command("_.-BLOCK", "TEST", "0,0", ss, "");

            ed.Command("_.-INSERT", "TEST", "0,0", 1, 1, 0);
        }
    } 
}