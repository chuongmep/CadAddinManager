using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;

namespace Test;

public class AddHatch
{
    [CommandMethod("AddHatch")]

    public static void TestAddHatch()

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

            BlockTableRecord acBlkTblRec;

            acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

            // Create a circle object for the closed boundary to hatch

            Circle acCirc = new Circle();

            acCirc.SetDatabaseDefaults();

            acCirc.Center = new Point3d(3, 3, 0);

            acCirc.Radius = 1;



            // Add the new circle object to the block table record and the transaction

            acBlkTblRec.AppendEntity(acCirc);

            acTrans.AddNewlyCreatedDBObject(acCirc, true);
            
            // Adds the circle to an object id array

            ObjectIdCollection acObjIdColl = new ObjectIdCollection();

            acObjIdColl.Add(acCirc.ObjectId);
            
            // Create the hatch object and append it to the block table record

            Hatch acHatch = new Hatch();

            acBlkTblRec.AppendEntity(acHatch);

            acTrans.AddNewlyCreatedDBObject(acHatch, true);
            
            // Set the properties of the hatch object

            // Associative must be set after the hatch object is appended to the 

            // block table record and before AppendLoop

            acHatch.SetDatabaseDefaults();

            acHatch.SetHatchPattern(HatchPatternType.PreDefined, "ANSI31");

            acHatch.Associative = true;

            acHatch.AppendLoop(HatchLoopTypes.Outermost, acObjIdColl);

            acHatch.EvaluateHatch(true);
            
            // Save the new object to the database

            acTrans.Commit();

        }
    }
}