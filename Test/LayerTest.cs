using System;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;

namespace Test;

public class LayerTest
{

    [CommandMethod("CreateLayer")]
    public void CreateLayer()
    {
        string name = "Test Layer" + new Random().Next(0, 255);
        Random random = new Random();
        System.Drawing.Color color = System.Drawing.Color.FromArgb((byte)random.Next(0, 255), (byte)random.Next(0, 255), (byte)random.Next(0, 255));
        AddLayer(name, color);
    }
    private static Database db = Application.DocumentManager.MdiActiveDocument.Database;

    public static void AddLayer(string layerName, System.Drawing.Color color)
    {
        using (Application.DocumentManager.MdiActiveDocument.LockDocument())
        {
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                LayerTable layerTable;
                LayerTableRecord layerTableRecord = new LayerTableRecord();

                layerTable = (LayerTable)db.LayerTableId.GetObject(OpenMode.ForWrite);

                layerTableRecord.Name = layerName;
                layerTableRecord.Color = Color.FromRgb(color.R, color.G, color.B);

                if (layerTable.Has(layerName) == false)
                {
                    layerTable.Add(layerTableRecord);

                    trans.AddNewlyCreatedDBObject(layerTableRecord, true);

                    trans.Commit();
                }

                    

            }
        }
    }
}