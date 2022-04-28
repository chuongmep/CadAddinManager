using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;
using Exception = System.Exception;

namespace Test
{
    public class TestLine
    {
        [CommandMethod("AutoCADSamples", "CreateLine", CommandFlags.Session)]
        public void CreateLineCommand()
        {
            var document = Application.DocumentManager.MdiActiveDocument;
            var database = document.Database;
            var editor = document.Editor;
            try
            {
                using (var transaction = database.TransactionManager.StartTransaction())
                {
                    // MessageBox.Show("dsds");
                    var blockTable = (BlockTable) transaction.GetObject(database.BlockTableId, OpenMode.ForRead);
                    var modelSpace =
                        (BlockTableRecord)
                        transaction.GetObject(blockTable[BlockTableRecord.ModelSpace], OpenMode.ForWrite);
                    Random r = new Random();
                    int x1 = r.Next(20,100);
                    var startPt = new Point3d(x1, 0,0);
                    var endPt = new Point3d(0, 0-x1,0);

                    var line = new Line(startPt, endPt);

                    modelSpace.AppendEntity(line);
                    transaction.AddNewlyCreatedDBObject(line, true);

                    transaction.Commit();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        [CommandMethod("FindCmdDuplicates")]
        public void FindCmdDuplicatesCmd()

        {
            string asmPath = SelectAssembly();
            if (string.IsNullOrEmpty(asmPath)) return;
            FindCmdDuplicates(asmPath);
        }


        private string SelectAssembly()
        {
            System.Windows.Forms.OpenFileDialog dlg =
                new System.Windows.Forms.OpenFileDialog();


            dlg.Title = "Load Assembly File";


            dlg.InitialDirectory = Environment.GetFolderPath(
                Environment.SpecialFolder.Desktop);


            dlg.Filter = ".Net Assembly (*.dll)|*.dll";

            dlg.FilterIndex = 1;

            dlg.RestoreDirectory = true;


            while (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)

            {
                try

                {
                    AssemblyName asmName =
                        AssemblyName.GetAssemblyName(dlg.FileName);


                    return dlg.FileName;
                }

                catch (BadImageFormatException)

                {
                    System.Windows.Forms.MessageBox.Show(
                        "Sorry the file is not a valid .Net assembly...",
                        "Invalid Assembly",
                        System.Windows.Forms.MessageBoxButtons.OK,
                        System.Windows.Forms.MessageBoxIcon.Error);
                }
            }


            return String.Empty;
        }


        public void FindCmdDuplicates(string asmPath)

        {
            Dictionary<string, List<MethodInfo>> map =
                new Dictionary<string, List<MethodInfo>>();


            Assembly asm = Assembly.LoadFile(asmPath);


            Type[] expTypes = asm.GetTypes();


            foreach (Type type in expTypes)

            {
                MethodInfo[] methods = type.GetMethods();


                foreach (MethodInfo method in methods)

                {
                    CommandMethodAttribute? attribute =
                        GetCommandMethodAttribute(method);
                    if (attribute == null) continue;
                    if (!map.ContainsKey(attribute.GlobalName))

                    {
                        var methodInfo = new List<MethodInfo>();


                        map.Add(attribute.GlobalName, methodInfo);
                    }


                    map[attribute.GlobalName].Add(method);
                }
            }

            Document doc = Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;
            foreach (var keyValuePair in map)
            {
                if (keyValuePair.Value.Count > 1)
                {
                    ed.WriteMessage(
                        "\nDuplicate Attribute: " + keyValuePair.Key);
                    foreach (var method in keyValuePair.Value)
                    {
                        ed.WriteMessage(
                            "\n – Method: " + method.Name);
                    }
                }
            }
        }

        public CommandMethodAttribute? GetCommandMethodAttribute(MethodInfo method)
        {
            object[] attributes = method.GetCustomAttributes(true);
            foreach (object attribute in attributes)
            {
                if (attribute is CommandMethodAttribute)
                {
                    return attribute as CommandMethodAttribute;
                }
            }

            return null;
        }
    }
}