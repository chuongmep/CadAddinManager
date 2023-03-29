using System;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using Application = Autodesk.AutoCAD.ApplicationServices.Application;


public class SetOrginHatch
{
    [CommandMethod("setOrginHatch")]
    public void setOriginHatch()
    {
        Document doc = Application.DocumentManager.MdiActiveDocument;
        Database db = doc.Database;
        Editor ed = doc.Editor;

        ObjectId mHatchId;
        Hatch mHatch = new Hatch();
        using (Transaction tr1 = db.TransactionManager.StartTransaction())
        {
            BlockTable bt = (BlockTable) tr1.GetObject(doc.Database.BlockTableId, OpenMode.ForRead);
            BlockTableRecord btr = (BlockTableRecord) tr1.GetObject(db.CurrentSpaceId, OpenMode.ForWrite);

            Point2d pt = new Point2d(0, 0);
            Polyline mPolyline = new Polyline(4);
            mPolyline.AddVertexAt(0, pt, 0.0, -1.0, -1.0);
            mPolyline.Normal = Vector3d.ZAxis;
            mPolyline.AddVertexAt(1, new Point2d(pt.X + 10, pt.Y), 0.0, -1.0, -1.0);
            mPolyline.AddVertexAt(2, new Point2d(pt.X + 10, pt.Y + 5), 0.0, -1.0, -1.0);
            mPolyline.AddVertexAt(3, new Point2d(pt.X, pt.Y + 5), 0.0, -1.0, -1.0);
            mPolyline.Closed = true;

            ObjectId mPlineId;
            mPlineId = btr.AppendEntity(mPolyline);
            tr1.AddNewlyCreatedDBObject(mPolyline, true);

            ObjectIdCollection ObjIds = new ObjectIdCollection();
            ObjIds.Add(mPlineId);

            Vector3d normal = new Vector3d(0.0, 0.0, 1.0);
            mHatch.Normal = normal;
            mHatch.Elevation = 0.0;
            mHatch.PatternScale = 2.0;
            mHatch.SetHatchPattern(HatchPatternType.PreDefined, "NET");
            mHatch.ColorIndex = 1;
            mHatch.PatternAngle = 2;

            //trying to set origin here does not work 
            //Point2d setOrigin = new Point2d(6.698, 2.78);
            //mHatch.Origin = setOrigin;

            btr.AppendEntity(mHatch);
            tr1.AddNewlyCreatedDBObject(mHatch, true);

            mHatch.Associative = true;
            mHatch.AppendLoop(HatchLoopTypes.Outermost, ObjIds);
            mHatch.EvaluateHatch(true);

            //get the ObjectId of hatch 
            mHatchId = mHatch.ObjectId;

            tr1.Commit();
        }

        //to set the origin use another transaction 
        using (Transaction tr2 = doc.TransactionManager.StartTransaction())
        {
            Entity ent = (Entity) tr2.GetObject(mHatchId, OpenMode.ForWrite);
            {
                Hatch? nHatch = ent as Hatch;
                string? hatchName = nHatch?.PatternName;
                Point2d setOrigin = new Point2d(6.698, 2.78);
                if (nHatch != null)
                {
                    nHatch.Origin = setOrigin;
                    nHatch.SetHatchPattern(HatchPatternType.PreDefined, hatchName);
                    nHatch.EvaluateHatch(true);
                    nHatch.Draw();
                }
            }

            tr2.Commit();
        }
    }
}