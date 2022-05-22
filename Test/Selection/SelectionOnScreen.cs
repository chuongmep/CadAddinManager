using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;

namespace Test.Selection;

public class SelectionOnScreen
{
    [CommandMethod("SelectObjectsOnscreen")]
    public static void SelectObjectsOnscreen()
    {
        // Get the current document and database
        Document acDoc = Application.DocumentManager.MdiActiveDocument;

        Database acCurDb = acDoc.Database;
        // Start a transaction

        using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
        {
            // Request for objects to be selected in the drawing area
            PromptSelectionResult acSSPrompt = acDoc.Editor.GetSelection();
            // If the prompt status is OK, objects were selected
            if (acSSPrompt.Status == PromptStatus.OK)
            {
                SelectionSet acSSet = acSSPrompt.Value;
                
                // Step through the objects in the selection set

                foreach (SelectedObject acSSObj in acSSet)
                {
                    // Check to make sure a valid SelectedObject object was returned
                    if (acSSObj != null)

                    {
                        // Open the selected object for write
                        Entity? acEnt = acTrans.GetObject(acSSObj.ObjectId, OpenMode.ForWrite) as Entity;
                        if (acEnt != null)
                        {
                            // Change the object's color to Green
                            acEnt.ColorIndex = 3;
                        }
                    }
                }
                // Save the new object to the database

                acTrans.Commit();
            }
            // Dispose of the transaction
        }
    }
}