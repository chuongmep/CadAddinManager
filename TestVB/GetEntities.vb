Imports Autodesk.AutoCAD.ApplicationServices
Imports Autodesk.AutoCAD.ApplicationServices.Core
Imports Autodesk.AutoCAD.DatabaseServices
Imports Autodesk.AutoCAD.Runtime

Public Class GetEntities
    <CommandMethod("ListEntities")>
    Public Sub ListEntities()
        '' Get the current document and database, and start a transaction
        Dim acDoc As Document = Application.DocumentManager.MdiActiveDocument
        Dim acCurDb As Database = acDoc.Database
        Using acTrans As Transaction = acCurDb.TransactionManager.StartTransaction()

            '' Open the Block table record for read
            Dim acBlkTbl As BlockTable
            acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId,
                                         OpenMode.ForRead)
            '' Open the Block table record Model space for read
            Dim acBlkTblRec As BlockTableRecord
            acBlkTblRec = acTrans.GetObject(acBlkTbl(BlockTableRecord.ModelSpace),OpenMode.ForRead)
            Dim nCnt As Integer = 0
            acDoc.Editor.WriteMessage(vbLf & "Model space objects: ")
            
            '' Step through each object in Model space and

            '' display the type of object found

            For Each acObjId As ObjectId In acBlkTblRec

                acDoc.Editor.WriteMessage(vbLf & acObjId.ObjectClass().DxfName)


                nCnt = nCnt + 1

            Next
            '' If no objects are found then display the following message
            If nCnt = 0 Then
                acDoc.Editor.WriteMessage(vbLf & "  No objects found")
            End If
            '' Dispose of the transaction
        End Using
    End Sub
End Class