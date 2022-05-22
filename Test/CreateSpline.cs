using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;

namespace Test;

public class CreateSpline
{
    [CommandMethod("AddSpline")]
    public static void AddSpline()
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

            acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace],
                OpenMode.ForWrite) as BlockTableRecord;
            // Define the fit points for the spline

            Point3dCollection ptColl = new Point3dCollection();

            ptColl.Add(new Point3d(0, 0, 0));

            ptColl.Add(new Point3d(5, 5, 0));

            ptColl.Add(new Point3d(10, 0, 0));
            
            // Get a 3D vector from the point (0.5,0.5,0)
            Vector3d vecTan = new Point3d(0.5, 0.5, 0).GetAsVector();
            // Create a spline through (0, 0, 0), (5, 5, 0), and (10, 0, 0) with a

            // start and end tangency of (0.5, 0.5, 0.0)

            Spline acSpline = new Spline(ptColl, vecTan, vecTan, 4, 0.0);

            acSpline.SetDatabaseDefaults();
            // Add the new object to the block table record and the transaction
            acBlkTblRec.AppendEntity(acSpline);
            acTrans.AddNewlyCreatedDBObject(acSpline, true);
            // Save the new line to the database

            acTrans.Commit();
        }
    }
}