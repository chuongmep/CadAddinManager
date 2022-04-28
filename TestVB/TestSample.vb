Imports System.Windows
Imports Autodesk.AutoCAD.Runtime

Public Class TestSample
    <CommandMethod("Hello World", CommandFlags.UsePickSet)>
    Public Sub HelloFunction()
    MessageBox.Show("Hello World")
    End Sub
End Class
