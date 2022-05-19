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
                }
            }
            catch (Exception e)
            {
                throw new ArgumentException(e.ToString());
            }
        }
        if(list.Count>0) return list.OrderBy(x=>x.Command.GlobalName).ToList();
        return list;
    }

    protected SortedDictionary<string, Addin> addinDict;

    protected int maxCount = 100;

    protected int count;
}