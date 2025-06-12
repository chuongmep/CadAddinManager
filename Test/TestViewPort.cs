using System.Collections.Generic;
using System.Windows.Forms;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;

namespace Test;

/// <summary>
/// https://forums.autodesk.com/t5/net-forum/viewport-get-entities-from-viewport/td-p/5832707
/// </summary>
public class TestViewPort
{

    [CommandMethod("TestInsideViewport")]
    public void TestInsideViewport()
    {
        Document doc = null;
        Database db = null;
        Editor ed = null;

        try
        {
            doc = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument;
            db = doc.Database;
            ed = doc.Editor;
            LayoutManager layoutManager = LayoutManager.Current;

            string layoutName = layoutManager.CurrentLayout;
            ed.WriteMessage($"\nLayout Name: {layoutName}");
            ObjectId layoutId = layoutManager.GetLayoutId(layoutName);

            ObjectId vpId = ObjectId.Null;
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                Layout layout = (Layout)tr.GetObject(layoutId, OpenMode.ForRead);
                var vpIds = layout.GetViewports();
                if (vpIds.Count > 1)
                    vpId = vpIds[1]; // First Viewport is Paperspace itself
                tr.Commit();
            }

            if (vpId.IsNull)
                throw new Exception(ErrorStatus.Vetoed, "viewport have issue");

            List<Point3d>? vpOutlinePntsInMs = null;
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                Viewport vp = (Viewport)tr.GetObject(vpId, OpenMode.ForRead);
                Polyline? vpOutlineInMs = null;

                if (vp.NonRectClipOn)
                {
                    ObjectId vpClipId = vp.NonRectClipEntityId;
                    Entity vpBoundary = (Entity)tr.GetObject(vpClipId, OpenMode.ForRead);
                    vpOutlineInMs = (Polyline)vpBoundary.Clone();
                }
                else
                {
                    Extents3d vpExt = vp.GeometricExtents;
                    vpOutlineInMs = new Polyline(4);
                    vpOutlineInMs.AddVertexAt(0, new Point2d(vpExt.MinPoint.X, vpExt.MinPoint.Y), 0, 0, 0);
                    vpOutlineInMs.AddVertexAt(1, new Point2d(vpExt.MaxPoint.X, vpExt.MinPoint.Y), 0, 0, 0);
                    vpOutlineInMs.AddVertexAt(2, new Point2d(vpExt.MaxPoint.X, vpExt.MaxPoint.Y), 0, 0, 0);
                    vpOutlineInMs.AddVertexAt(3, new Point2d(vpExt.MinPoint.X, vpExt.MaxPoint.Y), 0, 0, 0);
                    vpOutlineInMs.Closed = true;
                }

                Point3d center = new Point3d(vp.ViewCenter.X, vp.ViewCenter.Y, 0.0);
                Matrix3d msToPs =
                    Matrix3d.Displacement(new Vector3d(vp.CenterPoint.X - center.X, vp.CenterPoint.Y - center.Y, 0.0)) *
                    Matrix3d.Scaling(vp.CustomScale, center) *
                    Matrix3d.Rotation(vp.TwistAngle, Vector3d.ZAxis, Point3d.Origin) *
                    Matrix3d.WorldToPlane(new Plane(vp.ViewTarget, vp.ViewDirection));

                vpOutlineInMs.TransformBy(msToPs.Inverse());

                vpOutlinePntsInMs = new List<Point3d>();
                for (int i = 0; i < vpOutlineInMs.NumberOfVertices; i++)
                    vpOutlinePntsInMs.Add(vpOutlineInMs.GetPoint3dAt(i));

                // Draw polyline in model space
                Polyline transformedPoly = new Polyline();
                for (int i = 0; i < vpOutlinePntsInMs.Count; i++)
                    transformedPoly.AddVertexAt(i, new Point2d(vpOutlinePntsInMs[i].X, vpOutlinePntsInMs[i].Y), 0, 0,
                        0);
                transformedPoly.Closed = true;
                transformedPoly.ColorIndex = 1; // Red

                BlockTable bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                BlockTableRecord ms =
                    (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);
                ms.AppendEntity(transformedPoly);
                tr.AddNewlyCreatedDBObject(transformedPoly, true);

                tr.Commit();
            }


            int totalCount = 0;
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                BlockTableRecord blockTableRecord = (BlockTableRecord)tr.GetObject(SymbolUtilityServices.GetBlockModelSpaceId(db),
                    OpenMode.ForRead);
                foreach (ObjectId entId in blockTableRecord)
                {
                    Entity ent = (Entity)tr.GetObject(entId, OpenMode.ForRead);
                    if (ent is Polyline)
                        continue;

                    var pt1 = ent.GeometricExtents.MinPoint;
                    var pt2 = new Point3d(ent.GeometricExtents.MaxPoint.X, ent.GeometricExtents.MinPoint.Y, 0);
                    var pt3 = ent.GeometricExtents.MaxPoint;
                    var pt4 = new Point3d(ent.GeometricExtents.MinPoint.X, ent.GeometricExtents.MaxPoint.Y, 0);

                    if (IsInside2D(vpOutlinePntsInMs.ToArray(), pt1) &&
                        IsInside2D(vpOutlinePntsInMs.ToArray(), pt2) &&
                        IsInside2D(vpOutlinePntsInMs.ToArray(), pt3) &&
                        IsInside2D(vpOutlinePntsInMs.ToArray(), pt4))
                    {
                        if (ent is BlockReference block)
                        {
                            ed.WriteMessage($"\nEntity Block in Viewport: {entId.Handle} ({block.Name})");
                            totalCount += 1;
                        }

                    }
                }
                ed.WriteMessage($"Total block inside: {totalCount}");
                tr.Commit();
            }
        }
        catch (Exception ex)
        {
            if (ed != null)
                ed.WriteMessage("\n Error: " + ex.ToString());
            else
                MessageBox.Show("Error: " + ex.Message, "TestInsideViewport", MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
        }
    }
    private static bool IsInside2D(Point3d[] polyPoints, Point3d entPoint)
    {
        bool oddNodes = false;


        int i, j = polyPoints.Length - 1;

        for (i = 0; i < polyPoints.Length; i++)
        {
            Point3d pp_i = polyPoints[i];
            Point3d pp_j = polyPoints[j];

            if ((pp_i.Y < entPoint.Y && pp_j.Y >= entPoint.Y || pp_j.Y < entPoint.Y && pp_i.Y >= entPoint.Y) &&
                (pp_i.X <= entPoint.X || pp_j.X <= entPoint.X))
                oddNodes ^= (pp_i.X + (entPoint.Y - pp_i.Y) / (pp_j.Y - pp_i.Y) * (pp_j.X - pp_i.X) < entPoint.X);

            j = i;
        }

        return oddNodes;
    }

}