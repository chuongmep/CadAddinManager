using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.EditorInput;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;

namespace Test;

/// <summary>
/// https://adndevblog.typepad.com/autocad/2012/06/finding-all-xrefs-in-the-current-database-using-cnet.html
/// http://www.theswamp.org/index.php?topic=22884
/// https://forums.autodesk.com/t5/net-forum/get-list-of-xrefs-in-net/td-p/4553291
/// </summary>
public class XrefPathExtractor
{
    [CommandMethod("XrefGraph")]
    public static void XrefGraph()

    {
        Document doc = Application.DocumentManager.MdiActiveDocument;

        Database db = doc.Database;

        Editor ed = doc.Editor;

        using (Transaction Tx = db.TransactionManager.StartTransaction())

        {
            ed.WriteMessage("\n---Resolving the XRefs------------------");

            db.ResolveXrefs(true, false);

            XrefGraph xg = db.GetHostDwgXrefGraph(true);

            ed.WriteMessage("\n---XRef's Graph-------------------------");

            ed.WriteMessage("\nCURRENT DRAWING");

            GraphNode? root = xg.RootNode;

            PrintChildren(root, "|-------", ed, Tx);

            ed.WriteMessage("\n----------------------------------------\n");
        }
    }


    // Recursively prints out information about the XRef's hierarchy

    private static void PrintChildren(GraphNode? iRoot, string iIndent,
        Editor iEd, Transaction iTx)

    {
        for (int o = 0; o < iRoot!.NumOut; o++)

        {
            XrefGraphNode? child = iRoot.Out(o) as XrefGraphNode;

            if (child.XrefStatus == XrefStatus.Resolved)

            {
                BlockTableRecord? bl =
                    iTx.GetObject(child.BlockTableRecordId, OpenMode.ForRead)
                        as BlockTableRecord;

                iEd.WriteMessage("\n" + iIndent + child.Database.Filename);

                // Name of the Xref (found name)

                // You can find the original path too:

                //if (bl.IsFromExternalReference == true)

                // i_ed.WriteMessage("\n" + i_indent + "Xref path name: "

                //                      + bl.PathName);

                PrintChildren(child, "| " + iIndent, iEd, iTx);
            }
        }
    }
}