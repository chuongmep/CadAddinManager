using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;

namespace Test;

public class ScaleSelection
{
    [CommandMethod("ScaleSel")]
    public static void ScaleSelectedEntities()
    {
        var doc = Application.DocumentManager.MdiActiveDocument;
        var db = doc.Database;
        var ed = doc.Editor;

        var sel = ed.GetSelection();
        if (sel.Status != PromptStatus.OK) return;

        var baseRes = ed.GetPoint("\nBase point:");
        if (baseRes.Status != PromptStatus.OK) return;

        var scaleOpts = new PromptDoubleOptions("\nScale factor:") { AllowZero = false, AllowNegative = false };
        var scRes = ed.GetDouble(scaleOpts);
        if (scRes.Status != PromptStatus.OK) return;

        var mat = Matrix3d.Scaling(scRes.Value, baseRes.Value);

        using (var tr = db.TransactionManager.StartTransaction())
        {
            foreach (SelectedObject so in sel.Value)
            {
                var ent = tr.GetObject(so.ObjectId, OpenMode.ForWrite) as Entity;
                if (ent == null) continue;
                ent.TransformBy(mat);
            }
            tr.Commit();
        }
    }
}
