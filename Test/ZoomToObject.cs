using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using Application = Autodesk.AutoCAD.ApplicationServices.Application;

namespace Test;
/// <summary>
/// https://forums.autodesk.com/t5/net/zoom-to-objects/td-p/2788358
/// </summary>
public class ZoomToObject
{
    [CommandMethod("ZoomToObject")]
    public void Zoom()
    {
        Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
        PromptSelectionResult psr = ed.GetSelection();
        if (psr.Status != PromptStatus.OK)
            return;
        ObjectIdCollection idCol = new ObjectIdCollection(psr.Value.GetObjectIds());
        ZoomObjects(idCol);
    }

    private void ZoomObjects(ObjectIdCollection idCol)
    {
        Document doc = Application.DocumentManager.MdiActiveDocument;
        Database db = doc.Database;
        Editor ed = doc.Editor;
        using (Transaction tr = db.TransactionManager.StartTransaction())
        using (ViewTableRecord view = ed.GetCurrentView())
        {
            Matrix3d WCS2DCS = Matrix3d.PlaneToWorld(view.ViewDirection);
            WCS2DCS = Matrix3d.Displacement(view.Target - Point3d.Origin) * WCS2DCS;
            WCS2DCS = Matrix3d.Rotation(-view.ViewTwist, view.ViewDirection, view.Target) * WCS2DCS;
            WCS2DCS = WCS2DCS.Inverse();
            Entity ent = (Entity) tr.GetObject(idCol[0], OpenMode.ForRead);
            Extents3d ext = ent.GeometricExtents;
            ext.TransformBy(WCS2DCS);
            for (int i = 1; i < idCol.Count; i++)
            {
                ent = (Entity) tr.GetObject(idCol[i], OpenMode.ForRead);
                Extents3d tmp = ent.GeometricExtents;
                tmp.TransformBy(WCS2DCS);
                ext.AddExtents(tmp);
            }

            double ratio = view.Width / view.Height;
            double width = ext.MaxPoint.X - ext.MinPoint.X;
            double height = ext.MaxPoint.Y - ext.MinPoint.Y;
            if (width > (height * ratio))
                height = width / ratio;
            Point2d center =
                new Point2d((ext.MaxPoint.X + ext.MinPoint.X) / 2.0, (ext.MaxPoint.Y + ext.MinPoint.Y) / 2.0);
            view.Height = height;
            view.Width = width;
            view.CenterPoint = center;
            ed.SetCurrentView(view);
            tr.Commit();
        }
    }
}
