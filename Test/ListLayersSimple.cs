using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;

namespace Test;

public class ListLayersSimple
{
    [CommandMethod("ListLayersSimple")]
    public static void ListLayers()
    {
        var doc = Application.DocumentManager.MdiActiveDocument;
        var db = doc.Database;
        var ed = doc.Editor;

        using (var tr = db.TransactionManager.StartTransaction())
        {
            var lt = (LayerTable)tr.GetObject(db.LayerTableId, OpenMode.ForRead);
            foreach (var id in lt)
            {
                var ltr = (LayerTableRecord)tr.GetObject(id, OpenMode.ForRead);
                ed.WriteMessage($"\nLayer: {ltr.Name}");
            }
            tr.Commit();
        }
    }
}
