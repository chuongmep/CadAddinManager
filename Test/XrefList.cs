using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using acadApp = Autodesk.AutoCAD.ApplicationServices.Application;

namespace Test
{
    /// <summary>
    /// http://www.theswamp.org/index.php?topic=22884
    /// https://forums.autodesk.com/t5/net-forum/get-list-of-xrefs-in-net/td-p/4553291
    /// </summary>
    public static class ListXref
    {
        [CommandMethod("XRLST")]
        public static void GetXrefList()
        {
            Document doc = acadApp.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;
            XrefGraph xrg = db.GetHostDwgXrefGraph(true);
            GraphNodeCollection gnc = new GraphNodeCollection();
            for (int i = 0; i < xrg.NumNodes; i++)
            {
                gnc.Add(xrg.GetXrefNode(i));
            }
            for (int i = 1; i < xrg.NumNodes; i++)
            {
                XrefGraphNode xrgn = xrg.GetXrefNode(i);
                if (xrgn.IsNested)
                {
                    for (int j = 0; j < xrgn.NumIn; j++)
                    {
                        XrefGraphNode parent = xrg.GetXrefNode(gnc.IndexOf(gnc[i].In(j)));
                        ed.WriteMessage("\n{0}|{1} : {2}", parent.Name, xrgn.Name, getStatus(xrgn));
                    }
                }
                else
                {
                    ed.WriteMessage("\n{0} : {1}", xrgn.Name, getStatus(xrgn));
                }
            }
        }

        private static string getStatus(XrefGraphNode xrgn)
        {
            string result = string.Empty;
            switch (xrgn.XrefStatus)
            {
                case XrefStatus.FileNotFound:
                    result = "File not found";
                    break;
                case XrefStatus.Resolved:
                    result = "Resolved";
                    break;
                case XrefStatus.Unloaded:
                    result = "Unloaded";
                    break;
                case XrefStatus.Unreferenced:
                    result = "Unreferenced";
                    break;
                case XrefStatus.Unresolved:
                    result = "Unresolved";
                    break;
            }
            return result;
        }
    }
}