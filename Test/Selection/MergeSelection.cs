using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;

namespace Test.Selection;

public class MergeSelection
{
    [CommandMethod("MergeSelectionSets")]
    public static void MergeSelectionSets()
    {
        // Get the current document editor
        Editor acDocEd = Application.DocumentManager.MdiActiveDocument.Editor;
        // Request for objects to be selected in the drawing area

        PromptSelectionResult acSSPrompt;

        acSSPrompt = acDocEd.GetSelection();
        SelectionSet acSSet1;

        ObjectIdCollection acObjIdColl = new ObjectIdCollection();
        // If the prompt status is OK, objects were selected

        if (acSSPrompt.Status == PromptStatus.OK)

        {
            // Get the selected objects

            acSSet1 = acSSPrompt.Value;
            // Append the selected objects to the ObjectIdCollection

            acObjIdColl = new ObjectIdCollection(acSSet1.GetObjectIds());
        }
        // Request for objects to be selected in the drawing area

        acSSPrompt = acDocEd.GetSelection();
        SelectionSet acSSet2;


        // If the prompt status is OK, objects were selected

        if (acSSPrompt.Status == PromptStatus.OK)

        {
            acSSet2 = acSSPrompt.Value;


            // Check the size of the ObjectIdCollection, if zero, then initialize it

            if (acObjIdColl.Count == 0)

            {
                acObjIdColl = new ObjectIdCollection(acSSet2.GetObjectIds());
            }

            else

            {
                // Step through the second selection set

                foreach (ObjectId acObjId in acSSet2.GetObjectIds())

                {
                    // Add each object id to the ObjectIdCollection

                    acObjIdColl.Add(acObjId);
                }
            }
        }


        Application.ShowAlertDialog("Number of objects selected: " +
                                    acObjIdColl.Count.ToString());
    }
}