using System.Diagnostics;
using Autodesk.AutoCAD.Runtime;


public class DebugTraceTest
{
    [CommandMethod("DebugTraceTest")]
    public void Action()
    {
        //Trace or Debug Something
        Trace.WriteLine($"Warning: This is a warning");
        Trace.WriteLine($"Error: This is a error");
        Trace.WriteLine($"Add: This is a add");
        Trace.WriteLine($"Modify: This is a modify");
        Trace.WriteLine($"Delete: This is a delete");
    }
}