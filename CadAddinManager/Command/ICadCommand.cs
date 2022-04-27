namespace CadAddinManager.Command;

public abstract class ICadCommand
{
    /// <summary>
    /// Execute a function in cad application
    /// </summary>
    public abstract void Execute();
}