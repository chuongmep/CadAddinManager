using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using Application = Autodesk.AutoCAD.ApplicationServices.Application;

namespace Test;

public class EntityPickPointTest
{
    [CommandMethod("getEntityPickPoint")]
    public static void getEntityPickPoint()
    {
        Document doc = Application.DocumentManager.MdiActiveDocument;
        Database db = doc.Database;
        Editor ed = doc.Editor;

        PromptEntityOptions peo = new PromptEntityOptions("\nSelect Entity");
        PromptEntityResult per = ed.GetEntity(peo);
        if (per.Status == PromptStatus.OK)
        {
            Point3d pickedPoint = per.PickedPoint;
            Point3d pickedPtOsnap = ed.Snap("_near", pickedPoint);

            ed.WriteMessage("\n Selected pick point: {0}", pickedPoint.ToString());
            ed.WriteMessage("\n Selected Picked Point Osnap near: {0}", pickedPtOsnap);
        }
    }
}