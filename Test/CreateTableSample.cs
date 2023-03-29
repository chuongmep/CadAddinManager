using System;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using Application = Autodesk.AutoCAD.ApplicationServices.Application;

namespace Test;

public class CreateTableSample
{
    [CommandMethod("CRT")]
    [Obsolete("Obsolete")]
    public static void CreateTable()

    {
        Document doc = Application.DocumentManager.MdiActiveDocument;

        Database db = doc.Database;

        Editor ed = doc.Editor;

        PromptPointResult pr =
            ed.GetPoint("\nEnter table insertion point: ");

        if (pr.Status == PromptStatus.OK)

        {
            Table tb = new Table();

            tb.TableStyle = db.Tablestyle;

            tb.NumRows = 5;

            tb.NumColumns = 3;

            tb.SetRowHeight(3);

            tb.SetColumnWidth(15);

            tb.Position = pr.Value;

            // Create a 2-dimensional array

            // of our table contents

            string[,] str = new string[5, 3];

            str[0, 0] = "Part No.";

            str[0, 1] = "Name ";

            str[0, 2] = "Material ";

            str[1, 0] = "1876-1";

            str[1, 1] = "Flange";

            str[1, 2] = "Perspex";

            str[2, 0] = "0985-4";

            str[2, 1] = "Bolt";

            str[2, 2] = "Steel";

            str[3, 0] = "3476-K";

            str[3, 1] = "Tile";

            str[3, 2] = "Ceramic";

            str[4, 0] = "8734-3";

            str[4, 1] = "Kean";

            str[4, 2] = "Mostly water";

            // Use a nested loop to add and format each cell

            for (int i = 0; i < 5; i++)

            {
                for (int j = 0; j < 3; j++)

                {
                    tb.SetTextHeight(i, j, 1);

                    tb.SetTextString(i, j, str[i, j]);

                    tb.SetAlignment(i, j, CellAlignment.MiddleCenter);
                }
            }

            tb.GenerateLayout();
            Transaction tr = doc.TransactionManager.StartTransaction();
            using (tr)

            {
                BlockTable bt = (BlockTable) tr.GetObject(doc.Database.BlockTableId, OpenMode.ForRead);

                BlockTableRecord btr = (BlockTableRecord) tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

                btr.AppendEntity(tb);

                tr.AddNewlyCreatedDBObject(tb, true);

                tr.Commit();
            }
        }
    }
}