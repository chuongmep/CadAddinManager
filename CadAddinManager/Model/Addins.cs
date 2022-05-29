using System.IO;
using System.Reflection;
using Autodesk.AutoCAD.Runtime;
using Exception = System.Exception;

namespace CadAddinManager.Model;

public abstract class Addins
{
    public SortedDictionary<string, Addin> AddinDict
    {
        get => addinDict;
        set => addinDict = value;
    }

    public int Count => addinDict.Count;

    public Addins()
    {
        addinDict = new SortedDictionary<string, Addin>();
    }

    public void SortAddin()
    {
        foreach (var addin in addinDict.Values)
        {
            addin.SortAddinItem();
        }
    }

    public void AddAddIn(Addin addin)
    {
        var fileName = Path.GetFileName(addin.FilePath);
        if (addinDict.ContainsKey(fileName))
        {
            addinDict.Remove(fileName);
        }
        addinDict[fileName] = addin;
    }

    public bool RemoveAddIn(Addin addin)
    {
        var fileName = Path.GetFileName(addin.FilePath);
        if (addinDict.ContainsKey(fileName))
        {
            addinDict.Remove(fileName);
            return true;
        }
        return false;
    }

    public void AddItem(AddinItem item)
    {
        var assemblyName = item.AssemblyName;
        if (!addinDict.ContainsKey(assemblyName))
        {
            addinDict[assemblyName] = new Addin(item.AssemblyPath);
        }
        addinDict[assemblyName].ItemList.Add(item);
    }

    public List<AddinItem> LoadItems(Assembly assembly, string originalAssemblyFilePath, AddinType type)
    {
       
        List<AddinItem> list = new List<AddinItem>();
        if (assembly == null) return list;
        Type[] array;
        try
        {
            array = assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException ex)
        {
            array = ex.Types;
            if (array == null)
            {
                return list;
            }
        }
        foreach (var type2 in array)
        {
            try
            {
                List<MethodInfo> methodInfos = type2.GetMethods().ToList();
                foreach (MethodInfo methodInfo in methodInfos)
                {
                    switch (type)
                    {
                        case AddinType.Invalid:
                            break;
                        case AddinType.Command:
                            var commandAtt = methodInfo.GetCustomAttributes(typeof(CommandMethodAttribute), false).FirstOrDefault();
                            if (commandAtt != null)
                            {
                                CommandMethodAttribute attribute = commandAtt as CommandMethodAttribute;
                                string name = $"{methodInfo.DeclaringType.Name}.{methodInfo.Name}";
                                AddinItem item = new AddinItem(originalAssemblyFilePath, Guid.NewGuid(), name, type)
                                {
                                    MethodInfo = methodInfo,
                                    Command = attribute,
                                };
                                list.Add(item);
                            }
                            if(list.Count>0) list= list.OrderBy(x=>x.Command.GlobalName).ToList();
                            break;
                        case AddinType.LispFunction:
                            var lispFuncAtt = methodInfo.GetCustomAttributes(typeof(LispFunctionAttribute), false).FirstOrDefault();
                            if (lispFuncAtt != null)
                            {
                                LispFunctionAttribute attribute = lispFuncAtt as LispFunctionAttribute;
                                string name = $"{methodInfo.DeclaringType.Name}.{methodInfo.Name}";
                                AddinItem item = new AddinItem(originalAssemblyFilePath, Guid.NewGuid(), name, type)
                                {
                                    MethodInfo = methodInfo,
                                    LispFunction = attribute,
                                };
                                list.Add(item);
                            }
                            if(list.Count>0) list = list.OrderBy(x=>x.LispFunction.GlobalName).ToList();
                            break;
                        case AddinType.Mixed:
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(type), type, null);
                    }
                  
                }
            }
            catch (Exception e)
            {
                throw new ArgumentException(e.ToString());
            }
        }
        return list;
    }

    protected SortedDictionary<string, Addin> addinDict;

    protected int maxCount = 100;

    protected int count;
}