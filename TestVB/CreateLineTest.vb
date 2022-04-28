Imports Autodesk.AutoCAD.ApplicationServices
Imports Autodesk.AutoCAD.ApplicationServices.Core
Imports Autodesk.AutoCAD.DatabaseServices
Imports Autodesk.AutoCAD.Geometry
Imports Autodesk.AutoCAD.Runtime

Public Class CreateLineTest
    <CommandMethod("AddLine")>
    Public Sub AddLine()

        '' Get the current document and database
        Dim acDoc As Document = Application.DocumentManager.MdiActiveDocument

        Dim acCurDb As Database = acDoc.Database
        '' Start a transaction

        Using acTrans As Transaction = acCurDb.TransactionManager.StartTransaction()
            '' Open the Block table for read
            Dim acBlkTbl As BlockTable

            acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead)
            '' Open the Block table record Model space for write

            Dim acBlkTblRec As BlockTableRecord

            acBlkTblRec = acTrans.GetObject(acBlkTbl(BlockTableRecord.ModelSpace), _
                                            OpenMode.ForWrite)
            '' Create a line that starts at 5,5 and ends at 12,3
            Dim acLine As Line = New Line(New Point3d(5, 5, 0),New Point3d(12, 3, 0))
        acLine.SetDatabaseDefaults()
            
        '' Add the new object to the block table record and the transaction

        acBlkTblRec.AppendEntity(acLine)

        acTrans.AddNewlyCreatedDBObject(acLine, True)
        '' Save the new object to the database

        acTrans.Commit()

        End Using

    End Sub
End Class