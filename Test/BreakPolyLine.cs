using System.Linq;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;

namespace Test;

/// <summary>
/// https://forums.autodesk.com/t5/net/breaking-a-line-polyline/m-p/2488425
/// </summary>
public class BreakPolyLine
{
    [CommandMethod("BRLPL")]
    public void BreakLineOrPolyline()
    {
        var doc = Application.DocumentManager.MdiActiveDocument;
        var db = doc.Database;
        var ed = doc.Editor;

        var entityOptions = new PromptEntityOptions("\nSelect line or polyline: ");
        entityOptions.SetRejectMessage("Selected object is neither a line nor a polyline.");
        entityOptions.AddAllowedClass(typeof(Polyline), true);
        entityOptions.AddAllowedClass(typeof(Line), true);
        var entityResult = ed.GetEntity(entityOptions);
        if (entityResult.Status != PromptStatus.OK)
            return;

        Point3d pt1, pt2;
        var ucs = ed.CurrentUserCoordinateSystem;
        var pointOptions = new PromptPointOptions("\nSpecify second point or [First point]: ", "First");
        var pointResult = ed.GetPoint(pointOptions);
        if (pointResult.Status == PromptStatus.Keyword)
        {
            pointOptions.Message = "\nSpecify first point: ";
            pointOptions.Keywords.Clear();
            pointResult = ed.GetPoint(pointOptions);
            if (pointResult.Status != PromptStatus.OK)
                return;
            pt1 = pointResult.Value.TransformBy(ucs);
            pointOptions.Message = "\nSpecify second point: ";
            pointResult = ed.GetPoint(pointOptions);
            if (pointResult.Status != PromptStatus.OK)
                return;
            pt2 = pointResult.Value.TransformBy(ucs);
        }
        else if (pointResult.Status == PromptStatus.OK)
        {
            pt1 = entityResult.PickedPoint.TransformBy(ucs);
            pt2 = pointResult.Value.TransformBy(ucs);
        }
        else return;

        using (var tr = db.TransactionManager.StartTransaction())
        {
            var curve = (Curve) tr.GetObject(entityResult.ObjectId, OpenMode.ForWrite);
            double param1 = curve.GetParameterAtPoint(curve.GetClosestPointTo(pt1, false));
            double param2 = curve.GetParameterAtPoint(curve.GetClosestPointTo(pt2, false));
            var parameters = new DoubleCollection
            (new[] {param1, param2}
                .Distinct()
                .OrderBy(x => x)
                .ToArray());
            var curves = curve.GetSplitCurves(parameters);
            var curSpace = (BlockTableRecord) tr.GetObject(db.CurrentSpaceId, OpenMode.ForWrite);
            switch (curves.Count)
            {
                case 3:
                    curSpace.AppendEntity((Entity) curves[0]);
                    tr.AddNewlyCreatedDBObject(curves[0], true);
                    curSpace.AppendEntity((Entity) curves[2]);
                    tr.AddNewlyCreatedDBObject(curves[2], true);
                    curves[1].Dispose();
                    curve.Erase();
                    break;
                case 2:
                    curSpace.AppendEntity((Entity) curves[0]);
                    tr.AddNewlyCreatedDBObject(curves[0], true);
                    curSpace.AppendEntity((Entity) curves[1]);
                    tr.AddNewlyCreatedDBObject(curves[1], true);
                    curve.Erase();
                    break;
                default:
                    break;
            }

            tr.Commit();
        }
    }
}