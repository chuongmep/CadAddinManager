namespace CadAddinManager.Model;

[Flags]
public enum AddinType
{
    Invalid = 0,
    Command = 1,
    LispFunction = 2,
    Mixed = 3
}