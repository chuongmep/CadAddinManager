using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;

namespace Test;

public class SelectionFilters
{
    [CommandMethod("FilterSelectionSet")]
    public static void FilterSelectionSet()

    {
        // Get the current document editor

        Editor acDocEd = Application.DocumentManager.MdiActiveDocument.Editor;
        // Create a TypedValue array to define the filter criteria

        TypedValue[] acTypValAr = new TypedValue[1];

        acTypValAr.SetValue(new TypedValue((int) DxfCode.Start, "CIRCLE"), 0);
        // Assign the filter criteria to a SelectionFilter object

        SelectionFilter acSelFtr = new SelectionFilter(acTypValAr);
        // Request for objects to be selected in the drawing area

        PromptSelectionResult acSSPrompt;

        acSSPrompt = acDocEd.GetSelection(acSelFtr);


        // If the prompt status is OK, objects were selected

        if (acSSPrompt.Status == PromptStatus.OK)

        {
            SelectionSet acSSet = acSSPrompt.Value;

            Application.ShowAlertDialog("Number of objects selected: " + acSSet.Count.ToString());
        }
        else
        {
            Application.ShowAlertDialog("Number of objects selected: 0");
        }
    }
}