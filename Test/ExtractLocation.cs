using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using CsvHelper;
using Application = Autodesk.AutoCAD.ApplicationServices.Application;
using Exception = System.Exception;
using Trace = System.Diagnostics.Trace;

namespace Test;

public class ExtractLocation
{
    [CommandMethod("ExtractLocation")]
    public void GetAllXrefBlock()
    {
        // TestTransform();
        // return;
        Document doc = Application.DocumentManager.MdiActiveDocument;
        Editor ed = doc.Editor;
        Database database = doc.Database;

        List<AssignmentData> assignmentDatas = new List<AssignmentData>();
        using (Transaction tr = database.TransactionManager.StartTransaction())
        {
            PromptSelectionResult selRes = ed.GetSelection();
            if (selRes.Status == PromptStatus.OK)
            {
                SelectionSet selSet = selRes.Value;
                ObjectId[] objectIds = selSet.GetObjectIds();
                Trace.WriteLine("Number of selected objects: " + objectIds.Length);
                foreach (ObjectId objectId in objectIds)
                {
                    // Get the block table record
                    BlockReference blockReference =
                        (BlockReference) doc.TransactionManager.GetObject(objectId, OpenMode.ForRead);
                    // Get the block table record
                    GetInfoRecord(blockReference, ref assignmentDatas);
                }
            }

            tr.Commit();
        }

        // string filepath = @"D:/test.csv";
        // ExportToCsv(assignmentDatas, filepath, true, ",");
        // Process.Start(filepath);
    }

    void TestTransform()
    {
        try
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;
            Database database = doc.Database;
            PromptSelectionResult selRes = ed.GetSelection();
            Matrix3d wcs = doc.Editor.CurrentUserCoordinateSystem.Inverse();
            using Transaction tr = database.TransactionManager.StartTransaction();
            if (selRes.Status == PromptStatus.OK)
            {
                SelectionSet selSet = selRes.Value;
                ObjectId[] objectIds = selSet.GetObjectIds();
                ed.WriteMessage("User Picked :"+objectIds.Length.ToString());
                Point3d curPt = default;
                Point3d stepPt = default;
                foreach (ObjectId objectId in objectIds)
                {
                    BlockReference blockReference =
                        (BlockReference) doc.TransactionManager.GetObject(objectId, OpenMode.ForRead);
                    // Get position of block reference
                    Point3d position = blockReference.Position;
                    curPt = position.TransformBy(wcs);
                    // Get the block table record
                    BlockTableRecord tableRecord =
                        (BlockTableRecord) doc.TransactionManager.GetObject(blockReference.BlockTableRecord, OpenMode.ForRead);
                    // Get Entity of block table record
                    foreach (ObjectId id3 in tableRecord)
                    {
                        Entity entity3 = (Entity) doc.TransactionManager.GetObject(id3, OpenMode.ForRead);
                        if (entity3 is BlockReference)
                        {
                            BlockReference blockReference2 = (BlockReference) entity3;
                            // Get position of block reference
                            Point3d position2 = blockReference2.Position;
                            stepPt = position2.TransformBy(wcs);
                        }
                    }
                    // curPt add stepPt
                    curPt = curPt.Add(stepPt.GetAsVector());
                    MessageBox.Show(curPt.ToString());
                    return;
                    
                }
            }
            tr.Commit();
        }
        catch (Exception e)
        {
            MessageBox.Show(e.ToString());
        }
    }

    /// <summary>
    ///  return information of block record
    /// </summary>
    /// <param name="blockReference"></param>
    /// <param name="datas">list data </param>
    public void GetInfoRecord(BlockReference blockReference, ref List<AssignmentData> datas)
    {
        Document doc = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument;
        Editor editor = doc.Editor;
        Database database = doc.Database;
        Matrix3d wcs = doc.Editor.CurrentUserCoordinateSystem.Inverse();
        BlockTableRecord tableRecord =
            (BlockTableRecord) doc.TransactionManager.GetObject(blockReference.BlockTableRecord, OpenMode.ForRead);
        foreach (ObjectId id3 in tableRecord)
        {
            Entity entity3 = (Entity) doc.TransactionManager.GetObject(id3, OpenMode.ForRead);
            if (entity3 is BlockReference blockRef2)
            {
                GetInfoRecord(blockRef2, ref datas);
                Point3d curPt = default;
                Point3d stepPt = default;
                // Get Attribute of block
                string cap = String.Empty;
                string lid = String.Empty;
                Point3d? toolPosition = null;
                Point3d? lopcPosition = null;
                foreach (ObjectId id4 in blockRef2.AttributeCollection)
                {
                    AttributeReference attRef =
                        (AttributeReference) doc.TransactionManager.GetObject(id4, OpenMode.ForRead);
                    //if tag of attribute is "LPOC_ID" then get value of attribute
                    if (attRef.Tag == "LPOC_ID")
                    {
                        // Get position of block reference
                        Point3d position = blockReference.Position;
                        Matrix3d inverse = blockReference.BlockTransform.Inverse();
                        var total = wcs * inverse;
                        curPt = position.TransformBy(total);
                        stepPt = attRef.Position.TransformBy(wcs);
                        // curPt add stepPt
                        curPt = curPt.Add(stepPt.GetAsVector());
                        // rotate block 
                        datas.Add(new AssignmentData()
                        {
                            Name = attRef.TextString,
                            X = curPt.X.ToString(CultureInfo.InvariantCulture),
                            Y = curPt.Y.ToString(CultureInfo.InvariantCulture)
                        });
                        string textCheck = "F10A2-2-F43V-He-E43-64";
                        // string textCheck2 = "F10A2-2-F39V-He-E39-63(PT)";
                        if (attRef.TextString == textCheck)
                        {
                            MessageBox.Show(curPt.ToString());
                            
                            // Trace.WriteLine("LPOC Name: " + attRef.TextString);
                            // Trace.WriteLine($"LPOC Position: " + lopcPosition);
                        }
                    }

                    if (attRef.Tag == "MICAP")
                    {
                        cap = attRef.TextString;
                        Point3d position = blockReference.Position;
                        Matrix3d inverse = blockReference.BlockTransform.Inverse();
                        var total = wcs * inverse;
                        curPt = position.TransformBy(total);
                        stepPt = attRef.Position.TransformBy(wcs);
                        // curPt add stepPt
                        curPt = curPt.Add(stepPt.GetAsVector());
                    }

                    if (attRef.Tag == "LID")
                    {
                        lid = attRef.TextString;
                        cap = attRef.TextString;
                        Point3d position = blockReference.Position;
                        Matrix3d inverse = blockReference.BlockTransform.Inverse();
                        var total = wcs * inverse;
                        curPt = position.TransformBy(total);
                        stepPt = attRef.Position.TransformBy(wcs);
                        // curPt add stepPt
                        curPt = curPt.Add(stepPt.GetAsVector());
                    }
                }

                if (!string.IsNullOrEmpty(cap))
                {
                    string toolId = string.Join("-", lid, cap);
                    datas.Add(new AssignmentData()
                    {
                        Name = toolId,
                        X = curPt.X.ToString(CultureInfo.InvariantCulture),
                        Y = curPt.Y.ToString(CultureInfo.InvariantCulture)
                    });
                }
            }
        }
    }

    public static Point3d ConvertPointWcsToUcs(Point3d pointWcs)
    {
        Document doc = Application.DocumentManager.MdiActiveDocument;
        Database db = doc.Database;
        Editor ed = doc.Editor;

        // Get the current UCS
        Matrix3d ucsMatrix = ed.CurrentUserCoordinateSystem;

        // Invert the UCS matrix to get the WCS matrix
        Matrix3d wcsMatrix = ucsMatrix.Inverse();

        // Transform the point from WCS to UCS
        Point3d pointUcs = pointWcs.TransformBy(wcsMatrix);

        return pointUcs;
    }

    public static Point3d ConvertPointUcsToWcs(Point3d pointUcs)
    {
        Document doc = Application.DocumentManager.MdiActiveDocument;
        Editor ed = doc.Editor;

        // Get the current UCS
        Matrix3d ucsMatrix = ed.CurrentUserCoordinateSystem;

        // Transform the point from UCS to WCS
        Point3d pointWcs = pointUcs.TransformBy(ucsMatrix);

        return pointWcs;
    }

    public static void DrawPolyline(Point2d startPoint, Point2d endPoint, string layerName, Color layerColor,
        double lineWidth)
    {
        // Get the current document and database
        Document doc = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument;
        Database db = doc.Database;

        // Start a transaction
        using (Transaction tr = db.TransactionManager.StartTransaction())
        {
            // Open the Block table for read
            BlockTable bt = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;

            // Open the Block table record Model space for write
            BlockTableRecord btr = tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;
            // Create a new polyline
            Polyline pline = new Polyline();

            // Add the start and end points to the polyline
            pline.AddVertexAt(0, startPoint, 0, lineWidth, lineWidth);
            pline.AddVertexAt(1, endPoint, 0, lineWidth, lineWidth);

            // Set the layer for the polyline
            if (!bt.Has(layerName))
            {
                // Create the layer if it doesn't exist
                LayerTable lt = tr.GetObject(db.LayerTableId, OpenMode.ForWrite) as LayerTable;
                LayerTableRecord ltr = new LayerTableRecord();
                ltr.Name = layerName;
                ltr.Color = layerColor;
                lt.Add(ltr);
                tr.AddNewlyCreatedDBObject(ltr, true);
                pline.Layer = layerName;
            }
            else
            {
                pline.Layer = layerName;
            }

            // Add the polyline to the drawing
            btr.AppendEntity(pline);
            tr.AddNewlyCreatedDBObject(pline, true);

            // Commit the transaction
            tr.Commit();
        }
    }

    public void Example()
    {
        // Define the start and end points, layer name, layer color, and line weight
        Point2d startPoint = new Point2d(0, 0);
        Point2d endPoint = new Point2d(10, 10);
        string layerName = "0";
        Color layerColor = Color.FromColorIndex(ColorMethod.ByAci, 1);
        double lineWidth = 5.0;

        // Call the method to draw the polyline
        DrawPolyline(startPoint, endPoint, layerName, layerColor, lineWidth);
    }

    public static void ExportToCsv<T>(List<T> data, string filePath, bool includeHeader = true, string delimiter = ",")
    {
        var sb = new StringBuilder();

        // Get the property names from the object's properties
        PropertyInfo[] properties = typeof(T).GetProperties();
        if (includeHeader)
        {
            string header = string.Join(delimiter, properties.Select(p => p.Name));
            sb.AppendLine(header);
        }

        // Add the data rows to the CSV string
        foreach (T item in data)
        {
            string row = string.Join(delimiter, properties.Select(p => (p.GetValue(item, null) ?? "").ToString()));
            sb.AppendLine(row);
        }

        // Write the CSV string to a file and fix being used by another process
        using (var writer = new StreamWriter(filePath,false, Encoding.UTF8))
        {
            writer.Write(sb.ToString());
        }
    }

    public static void ExportToCsv2<T>(List<T> data, string filePath, bool includeHeader = true, string delimiter = ",")
    {
        using (var writer = new StreamWriter(filePath))
        using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
        {
            if (includeHeader)
            {
                // Write the header row
                csv.WriteHeader<T>();
                csv.NextRecord();
            }

            // Write the data rows
            csv.WriteRecords(data);
        }
    }

    public class AssignmentData
    {
        public string Name { get; set; }
        public string X { get; set; }
        public string Y { get; set; }
    }
}