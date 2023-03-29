using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Windows;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using CadApp = Autodesk.AutoCAD.ApplicationServices.Application;
namespace Test;

public class ExtractFileFromResources

{
    [CommandMethod("InsertBlockFromResources")]
    public void Execute()
    {
        // this path test is from folder FileSample published
        string path = @"D:\API\CAD\CadAddinManager\Test\FileSample\sample_base.dwg";
        InsertBlock(path, new Point3d(0, 0, 0));
    }

    [CommandMethod("GetResource")]
    public void GetResource()
    {
        string[] resourceNames = Assembly.GetExecutingAssembly().GetManifestResourceNames();
        foreach(string resourceName in resourceNames)
        {
            //this one return Test.g.resources
            MessageBox.Show(resourceName);
        }
    }

    [CommandMethod("ExtractFile")]
    public static void RunDocumentCommand()
    {
        var dwg = CadApp.DocumentManager.MdiActiveDocument;
        var ed = dwg.Editor;

        try
        {
            var blkFile = ExtractFileFromResource("MyBlock");
            if (!string.IsNullOrEmpty(blkFile))
            {
                //Use this drawing file from AutoCAD's temp file location
                // such as insert this block file into current drawing
                CadApp.ShowAlertDialog(
                    "Block file saved in project resources has been extracted!");

                File.Delete(blkFile);
            }
        }
        catch (System.Exception ex)
        {
            ed.WriteMessage("\nError: {0}", ex.Message);
            ed.WriteMessage("\nStack Trace: {0}", ex.StackTrace);
        }
        finally
        {
            Autodesk.AutoCAD.Internal.Utils.PostCommandPrompt();
        }
    }
    public static string ExtractFileFromResource(string resourceName)
    {
        dynamic preferences = CadApp.Preferences;
        string filePathName = 
            (string)preferences.Files.TempFilePath + "\\" + resourceName + ".dwg";
        try
        {
            ResourceManager ResourceManager = new ResourceManager("Test.g.resources", Assembly.GetExecutingAssembly());
            var bytes = (byte[])ResourceManager.GetObject(resourceName);
            if (File.Exists(filePathName)) File.Delete(filePathName);
            using (var stream = new FileStream(filePathName, FileMode.Create, FileAccess.Write))
            {
                stream.Write(bytes, 0, bytes.Length);
                stream.Close();
            }
            return filePathName;
        }
        catch
        {
            return "";
        }
    }

    private static ObjectId InsertBlock(string path, Point3d loc)
    {
        ObjectId retId;

        Document AcDocument =
            Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument;
        Database AcDatabase = AcDocument.Database;

        using (AcDocument.LockDocument())
        {
            using (Transaction tr = AcDocument.TransactionManager.StartTransaction())
            {
                if (tr == null) throw new ArgumentNullException(nameof(tr));
                Scale3d scl = new Scale3d(AcDatabase.Cannoscale.DrawingUnits, AcDatabase.Cannoscale.DrawingUnits,
                    AcDatabase.Cannoscale.DrawingUnits);
                BlockTable? bt = tr.GetObject(AcDatabase.BlockTableId, OpenMode.ForRead) as BlockTable;
                if (bt == null) throw new ArgumentNullException(nameof(bt));
                if (bt.Has(Path.GetFileNameWithoutExtension(path)))
                {
                    //Block exists in file already
                    BlockTableRecord? btr =
                        tr.GetObject(bt[Path.GetFileNameWithoutExtension(path)], OpenMode.ForRead) as
                            BlockTableRecord;
                    if (btr == null) throw new ArgumentNullException(nameof(btr));
                    BlockReference ent = new BlockReference(loc, btr.ObjectId);
                    ent.ScaleFactors = scl;

                    BlockTableRecord modelspace =
                        tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;
                    modelspace.AppendEntity(ent);
                    tr.AddNewlyCreatedDBObject(ent, true);

                    retId = ent.ObjectId;

                    if (btr != null)
                    {
                        Dictionary<AttributeReference, AttributeDefinition> dict =
                            new Dictionary<AttributeReference, AttributeDefinition>();
                        if (btr.HasAttributeDefinitions)
                        {
                            foreach (ObjectId id in btr)
                            {
                                DBObject obj = tr.GetObject(id, OpenMode.ForRead);
                                if (obj is AttributeDefinition)
                                {
                                    AttributeDefinition ad = obj as AttributeDefinition;
                                    AttributeReference ar = new AttributeReference();
                                    ar.SetAttributeFromBlock(ad, ent.BlockTransform);

                                    ent.AttributeCollection.AppendAttribute(ar);
                                    tr.AddNewlyCreatedDBObject(ar, true);

                                    dict.Add(ar, ad);
                                }
                            }
                        }
                    }
                }
                else
                {
                    // Block does not exist in file, need to insert first
                    // Some code from:
                    // https://forums.autodesk.com/t5/net/insert-block-from-another-file/td-p/3527080
                    ObjectId ObjId;
                    BlockTableRecord? modelspace =
                        tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                    using (Database dbInsert = new Database(false, true))
                    {
                        dbInsert.ReadDwgFile(path, System.IO.FileShare.Read, true, "");
                        ObjId = AcDatabase.Insert(Path.GetFileNameWithoutExtension(path), dbInsert, true);

                        BlockReference BlkRef = new BlockReference(loc, ObjId);
                        BlkRef.ScaleFactors = scl;

                        modelspace.AppendEntity(BlkRef);
                        tr.AddNewlyCreatedDBObject(BlkRef, true);

                        retId = BlkRef.ObjectId;

                        BlockTableRecord? btr =
                            tr.GetObject(bt[Path.GetFileNameWithoutExtension(path)], OpenMode.ForRead) as
                                BlockTableRecord;
                        if (btr != null)
                        {
                            Dictionary<AttributeReference, AttributeDefinition> dict =
                                new Dictionary<AttributeReference, AttributeDefinition>();
                            if (btr.HasAttributeDefinitions)
                            {
                                foreach (ObjectId id in btr)
                                {
                                    DBObject obj = tr.GetObject(id, OpenMode.ForRead);
                                    if (obj is AttributeDefinition)
                                    {
                                        AttributeDefinition? ad = obj as AttributeDefinition;
                                        if(ad==null) continue;
                                        AttributeReference ar = new AttributeReference();
                                        ar.SetAttributeFromBlock(ad, BlkRef.BlockTransform);

                                        BlkRef.AttributeCollection.AppendAttribute(ar);
                                        tr.AddNewlyCreatedDBObject(ar, true);

                                        dict.Add(ar, ad);
                                    }
                                }
                            }
                        }
                    }
                }

                tr.Commit();
            }
        }

        return retId;
    }
}