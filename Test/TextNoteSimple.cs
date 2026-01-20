using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;

namespace Test;

public class TextNoteSimple
{
    [CommandMethod("TextNote")]
    public static void CreateTextNote()
    {
        var doc = Application.DocumentManager.MdiActiveDocument;
        var db = doc.Database;
        var ed = doc.Editor;

        var pRes = ed.GetPoint("\nInsertion point:");
        if (pRes.Status != PromptStatus.OK) return;
        var sRes = ed.GetString("\nText content:");
        if (sRes.Status != PromptStatus.OK) return;

        using (var tr = db.TransactionManager.StartTransaction())
        {
            var bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
            var ms = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

            var txt = new DBText
            {
                Position = pRes.Value,
                Height = 2.5,
                TextString = sRes.StringResult
            };
            ms.AppendEntity(txt);
            tr.AddNewlyCreatedDBObject(txt, true);
            tr.Commit();
        }
    }
}
