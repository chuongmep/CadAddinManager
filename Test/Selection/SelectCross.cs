using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;

namespace Test;

public class SelectCross
{
    [CommandMethod("SelectObjectsByCrossingWindow")]
    public static void SelectObjectsByCrossingWindow()
    {
        // Get the current document editor
        Editor acDocEd = Application.DocumentManager.MdiActiveDocument.Editor;
        // Create a crossing window from (2,2,0) to (10,8,0)
        PromptSelectionResult acSSPrompt;
        acSSPrompt = acDocEd.SelectCrossingWindow(new Point3d(0, 0, 0),
            new Point3d(10, 8, 0));
        // If the prompt status is OK, objects were selected

        if (acSSPrompt.Status == PromptStatus.OK)

        {
            SelectionSet acSSet = acSSPrompt.Value;
            Application.ShowAlertDialog("Number of objects selected: " +
                                        acSSet.Count.ToString());
        }
        else

        {
            Application.ShowAlertDialog("Number of objects selected: 0");
        }
    }
}