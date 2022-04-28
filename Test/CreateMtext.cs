using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;

public class CreateMtext
{
    [CommandMethod("TESTMTEXT")]
    public static void TestMtextCreate()
    {
        Document doc = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument;
        if (doc == null)
            return;
        try
        {
            using (Transaction tr = doc.TransactionManager.StartTransaction())
            {
                MText mytext = new MText();
                mytext.SetDatabaseDefaults();
                mytext.Contents = "mytext";
                mytext.Layer = "0";
                mytext.ColorIndex = 3;
                mytext.Location = new Point3d(0.0, 0.0, 0.0);
                mytext.Width = 0.0; // don't forget me 
                if (mytext.Width > 0.0)
                    mytext.ColumnType = ColumnType.NoColumns;

                BlockTable bt = (BlockTable) tr.GetObject(doc.Database.BlockTableId, OpenMode.ForRead);
                BlockTableRecord btr =
                    (BlockTableRecord) tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);
                btr.AppendEntity(mytext);
                tr.AddNewlyCreatedDBObject(mytext, true);
                tr.Commit();
            }
        }
        catch (System.Exception ex)
        {
            doc.Editor.WriteMessage("\nError: " + ex.ToString());
        }
    }
}