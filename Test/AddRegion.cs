using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using Application = Autodesk.AutoCAD.ApplicationServices.Application;

namespace Test;

public class AddRegion
{
    [CommandMethod("AddRegion")]
    public static void TestAddRegion()

    {
        // Get the current document and database

        Document acDoc = Application.DocumentManager.MdiActiveDocument;

        Database acCurDb = acDoc.Database;


        // Start a transaction

        using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())

        {
            // Open the Block table for read

            BlockTable? acBlkTbl;

            acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead) as BlockTable;
            // Open the Block table record Model space for write

            BlockTableRecord? acBlkTblRec = null;

            if (acBlkTbl != null)
                acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace],
                    OpenMode.ForWrite) as BlockTableRecord;


            // Create an in memory circle

            using (Circle acCirc = new Circle())

            {
                acCirc.SetDatabaseDefaults();

                acCirc.Center = new Point3d(2, 2, 0);

                acCirc.Radius = 5;


                // Adds the circle to an object array

                DBObjectCollection acDBObjColl = new DBObjectCollection();

                acDBObjColl.Add(acCirc);


                // Calculate the regions based on each closed loop

                DBObjectCollection myRegionColl = new DBObjectCollection();

                myRegionColl = Region.CreateFromCurves(acDBObjColl);

                Region? acRegion = myRegionColl[0] as Region;


                // Add the new object to the block table record and the transaction

                acBlkTblRec?.AppendEntity(acRegion);

                acTrans.AddNewlyCreatedDBObject(acRegion, true);


                // Dispose of the in memory circle not appended to the database
            }


            // Save the new object to the database

            acTrans.Commit();
        }
    }
}