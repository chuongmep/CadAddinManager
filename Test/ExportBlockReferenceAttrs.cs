using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using Application = Autodesk.AutoCAD.ApplicationServices.Application;

namespace Test;

/// <summary>
/// Testing How to get block attribute from autocad.
/// https://stackoverflow.com/questions/68068857/autocad-2021-net-api-getting-all-blockreferences-in-a-database
///
/// </summary>
public class ExportBlockReferenceAttrs
{
    [CommandMethod("Export Block Attrs")]
    public void Export()
    {
        Document doc = Application.DocumentManager.MdiActiveDocument;
        Editor editor = doc.Editor;
        Database db = doc.Database;
        PromptSelectionResult psr = editor.GetSelection();
        if (psr.Status != PromptStatus.OK)
            return;
        ObjectIdCollection idCol = new ObjectIdCollection(psr.Value.GetObjectIds());
        editor.WriteMessage($"Count Ids {idCol.Count}");
        using Transaction tr = db.TransactionManager.StartTransaction();
        foreach (ObjectId entityId in idCol)
        {
            Entity? entity = tr.GetObject(entityId, OpenMode.ForRead) as Entity;
            if (entity is BlockReference blockRef)
            {
                if (blockRef.IsDynamicBlock)
                {
                    DynamicBlockReferencePropertyCollection pc = blockRef.DynamicBlockReferencePropertyCollection;
                    foreach (DynamicBlockReferenceProperty blockReferenceProperty in pc)
                    {
                        editor.WriteMessage(
                            $"name:{blockReferenceProperty.PropertyName}, value:{blockReferenceProperty.Value}\n");
                    }

                    string blockName = blockRef.Name;
                    editor.WriteMessage($"block Name:{blockName}, hander{blockRef.Handle.Value}");
                }
                else
                {
                    if (blockRef.AttributeCollection.Count > 0)
                    {
                        foreach (ObjectId attId in blockRef.AttributeCollection)
                        {
                            AttributeReference? attRef = tr.GetObject(attId, OpenMode.ForRead) as AttributeReference;
                            if (attRef != null)
                            {
                                editor.WriteMessage($"\n   -> {attRef.Tag} = {attRef.TextString}\n");
                            }
                        }
                    }
                }
            }
        }

        tr.Abort();
    }

    [CommandMethod("EXPORTBLOCKS")]
    public void ExportAllBlocks()
    {
        Document doc = Application.DocumentManager.MdiActiveDocument;
        Editor ed = doc.Editor;
        Database db = doc.Database;

        using (Transaction tr = db.TransactionManager.StartTransaction())
        {
            BlockTable bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
            LayoutManager lm = LayoutManager.Current;
            string currentLayoutName = lm.CurrentLayout;

            DBDictionary layoutDict = (DBDictionary)tr.GetObject(db.LayoutDictionaryId, OpenMode.ForRead);
            Layout? layout = null;
            foreach (DBDictionaryEntry entry in layoutDict)
            {
                Layout l = (Layout)tr.GetObject(entry.Value, OpenMode.ForRead);
                if (l.LayoutName == currentLayoutName)
                {
                    layout = l;
                    break;
                }
            }

            if (layout == null)
            {
                ed.WriteMessage("Layout not found.");
                return;
            }

            BlockTableRecord btr = (BlockTableRecord)tr.GetObject(layout.BlockTableRecordId, OpenMode.ForRead);

            int count = 0;
            foreach (ObjectId objId in btr)
            {
                Entity? ent = tr.GetObject(objId, OpenMode.ForRead) as Entity;
                if (ent is BlockReference blockRef)
                {
                    count++;
                    ed.WriteMessage($"\n[{count}] Block Name: {blockRef.Name}, Handle: {blockRef.Handle}");

                    if (blockRef.IsDynamicBlock)
                    {
                        DynamicBlockReferencePropertyCollection
                            props = blockRef.DynamicBlockReferencePropertyCollection;
                        foreach (DynamicBlockReferenceProperty prop in props)
                        {
                            ed.WriteMessage($"\n    -> DynamicProp: {prop.PropertyName} = {prop.Value}");
                        }
                    }

                    if (blockRef.AttributeCollection.Count > 0)
                    {
                        foreach (ObjectId attId in blockRef.AttributeCollection)
                        {
                            AttributeReference? attRef = tr.GetObject(attId, OpenMode.ForRead) as AttributeReference;
                            if (attRef != null)
                            {
                                ed.WriteMessage($"\n    -> Attribute: {attRef.Tag} = {attRef.TextString}");
                            }
                        }
                    }
                }
            }

            ed.WriteMessage($"\nTotal BlockReference trong layout '{currentLayoutName}': {count}");
            tr.Commit();
        }
    }
}