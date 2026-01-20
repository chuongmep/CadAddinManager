using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;

namespace Test;

public class CreateLayerSimple
{
    [CommandMethod("MakeLayer")]
    public static void MakeLayer()
    {
        var doc = Application.DocumentManager.MdiActiveDocument;
        var db = doc.Database;
        var ed = doc.Editor;

        var sRes = ed.GetString("\nNew layer name:");
        if (sRes.Status != Autodesk.AutoCAD.EditorInput.PromptStatus.OK) return;
        var name = sRes.StringResult;

        using (var tr = db.TransactionManager.StartTransaction())
        {
            var lt = (LayerTable)tr.GetObject(db.LayerTableId, OpenMode.ForWrite);
            if (lt.Has(name))
            {
                ed.WriteMessage($"\nLayer '{name}' already exists.");
            }
            else
            {
                var ltr = new LayerTableRecord { Name = name, Color = Color.FromColorIndex(ColorMethod.ByAci, 3) };
                lt.Add(ltr);
                tr.AddNewlyCreatedDBObject(ltr, true);
                ed.WriteMessage($"\nLayer '{name}' created.");
            }
            tr.Commit();
        }
    }
}
