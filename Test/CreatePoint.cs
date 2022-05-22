using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;

namespace Test;

public class CreatePoint
{
    [CommandMethod("AddPointAndSetPointStyle")]
    public static void AddPointAndSetPointStyle()
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
            // Create a point at (4, 3, 0) in Model space

            DBPoint acPoint = new DBPoint(new Point3d(4, 3, 0));
            acPoint.SetDatabaseDefaults();
            // Add the new object to the block table record and the transaction
            acBlkTblRec.AppendEntity(acPoint);
            acTrans.AddNewlyCreatedDBObject(acPoint, true);
            // Set the style for all point objects in the drawing
            acCurDb.Pdmode = 34;
            acCurDb.Pdsize = 1;
            // Save the new object to the database
            acTrans.Commit();
        }
    }
}