using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using Application = Autodesk.AutoCAD.ApplicationServices.Application;

namespace Test;

public class ExportBlockCoordinate
{
    [CommandMethod("ExportBlockCoordinate")]
    public void Export()
    {
        Document doc = Application.DocumentManager.MdiActiveDocument;
        Editor editor = doc.Editor;
        Database db = doc.Database;

        // Prompt the user for the layer name
        PromptResult promptResult = editor.GetString("\nEnter the layer name: ");
        if (promptResult.Status != PromptStatus.OK)
        {
            editor.WriteMessage("\nInvalid input. Layer name not provided.");
            return;
        }
        string layerName = promptResult.StringResult;

        // Display the Save File dialog
        SaveFileDialog saveFileDialog = new SaveFileDialog();
        saveFileDialog.Filter = "CSV Files (*.csv)|*.csv";
        saveFileDialog.Title = "Save Block Information";
        // save file name with current datetime 
        saveFileDialog.FileName = $"BlockCoordinate_{DateTime.Now.ToString("yyyyMMddHHmmss")}";
        DialogResult dialogResult = saveFileDialog.ShowDialog();

        if (dialogResult != DialogResult.OK)
        {
            editor.WriteMessage("\nInvalid input. Output file path not provided.");
            return;
        }
        string filePath = saveFileDialog.FileName;

        using (Transaction tr = db.TransactionManager.StartTransaction())
        {
            // Open the block table for read
            BlockTable blockTable = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;

            // Open the current space block table record for read
            BlockTableRecord spaceRecord = tr.GetObject(db.CurrentSpaceId, OpenMode.ForRead) as BlockTableRecord;

            using (StreamWriter writer = new StreamWriter(filePath))
            {
                // Write the CSV header
                writer.WriteLine("Block Name,Anonymous Name,X,Y,Z,Rotation");

                // Iterate through all the entities in the current space
                foreach (ObjectId entityId in spaceRecord)
                {
                    Entity entity = tr.GetObject(entityId, OpenMode.ForRead) as Entity;

                    // Check if the entity is a block reference and on the specified layer
                    if (entity is BlockReference blockRef && blockRef.Layer.Equals(layerName, StringComparison.OrdinalIgnoreCase))
                    {
                        string AnonymousName = String.Empty;
                        // Get name of anonymous block
                        if (blockRef.IsDynamicBlock)
                        {
                            BlockTableRecord? blockTableRecord = tr.GetObject(blockRef.DynamicBlockTableRecord, OpenMode.ForRead) as BlockTableRecord;
                            AnonymousName = blockTableRecord?.Name?? String.Empty;
                            string blockName = blockRef.Name;
                            Point3d location = blockRef.Position;
                            double rotation = ToDeg(blockRef.Rotation);
                            // Write the block information to the CSV file
                            writer.WriteLine($"{blockName},{AnonymousName},{location.X},{location.Y},{location.Z},{rotation}");
                            editor.WriteMessage(blockName);
                        }
                        else
                        {
                            string blockName = blockRef.Name;
                            Point3d location = blockRef.Position;
                            double rotation = ToDeg(blockRef.Rotation);
                            // Write the block information to the CSV file
                            writer.WriteLine($"{blockName},{AnonymousName},{location.X},{location.Y},{location.Z},{rotation}");
                        }
                    }
                }
            }

            tr.Commit();
        }
        editor.WriteMessage($"\nBlock information exported to: {filePath}");
        Process.Start(filePath);

    }
    double ToDeg(double rad)
    {
        return rad * (180.0 / Math.PI);
    }
}