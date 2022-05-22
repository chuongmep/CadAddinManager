using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;

namespace Test.Selection;

public class PickSelection
{
    [CommandMethod("CheckForPickfirstSelection", CommandFlags.UsePickSet)]
    public static void CheckForPickfirstSelection()

    {
        // Get the current document
        Editor acDocEd = Application.DocumentManager.MdiActiveDocument.Editor;
        // Get the PickFirst selection set

        PromptSelectionResult acSSPrompt;
        acSSPrompt = acDocEd.SelectImplied();
        SelectionSet acSSet;
        // If the prompt status is OK, objects were selected before

        // the command was started

        if (acSSPrompt.Status == PromptStatus.OK)

        {
            acSSet = acSSPrompt.Value;
            Application.ShowAlertDialog("Number of objects in Pickfirst selection: " + acSSet.Count.ToString());
        }
        else

        {
            Application.ShowAlertDialog("Number of objects in Pickfirst selection: 0");
        }

        // Clear the PickFirst selection set

        ObjectId[] idarrayEmpty = new ObjectId[0];

        acDocEd.SetImpliedSelection(idarrayEmpty);


        // Request for objects to be selected in the drawing area
        acSSPrompt = acDocEd.GetSelection();
        // If the prompt status is OK, objects were selected

        if (acSSPrompt.Status == PromptStatus.OK)
        {
            acSSet = acSSPrompt.Value;
            Application.ShowAlertDialog("Number of objects selected: " + acSSet.Count.ToString());
        }
        else

        {
            Application.ShowAlertDialog("Number of objects selected: 0");
        }
    }
}