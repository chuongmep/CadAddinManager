using System;
using System.Windows.Forms;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;

namespace Test;

public class ArcTest
{
    [CommandMethod("AutoCADSamples", "CreateArc", CommandFlags.Modal)]
    public void CreateArcCommand()
    {
        var document = Application.DocumentManager.MdiActiveDocument;
        var database = document.Database;

        try
        {
            using (var transaction = database.TransactionManager.StartTransaction())
            {
                var blockTable = (BlockTable)transaction.GetObject(database.BlockTableId, OpenMode.ForRead);
                var modelSpace = (BlockTableRecord)transaction.GetObject(blockTable[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

                Random random = new Random();
                int c = random.Next(5, 100);
                var centerPt = new Point3d(0.0, 0.0, 0.0);
                var radius = c;

                var arc = new Arc(centerPt, Vector3d.ZAxis, radius, 0.0, Math.PI / 2);

                modelSpace.AppendEntity(arc);
                transaction.AddNewlyCreatedDBObject(arc, true);

                transaction.Commit();
            }
        }
        catch (System.Exception ex)
        {
            MessageBox.Show(ex.Message);
        }
    }
}