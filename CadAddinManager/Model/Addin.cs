using CadAddinManager.Command;

namespace CadAddinManager.Model;

public class Addin : IAddinNode
{
    public List<AddinItem> ItemList
    {
        get => itemList;
        set => itemList = value;
    }

    public string FilePath
    {
        get => filePath;
        set => filePath = value;
    }

    public bool Save
    {
        get => save;
        set => save = value;
    }

    public bool Hidden
    {
        get => hidden;
        set => hidden = value;
    }

    public Addin(string filePath)
    {
        itemList = new List<AddinItem>();
        this.filePath = filePath;
        save = true;
    }

    public Addin(string filePath, List<AddinItem> itemList)
    {
        this.itemList = itemList;
        this.filePath = filePath;
        SortAddinItem();
        save = true;
    }

    public void SortAddinItem()
    {
        itemList.Sort(new AddinItemComparer());
    }

    public void RemoveItem(AddinItem item)
    {
        itemList.Remove(item);
        if (itemList.Count == 0)
        {
            AddinManagerBase.Instance.AddinManager.RemoveAddin(this);
        }
    }

    public void SaveToLocalIni(IniFile file)
    {
        if (itemList == null || itemList.Count == 0)
        {
            return;
        }
        file.WriteSection("ExternalCommands");
        file.Write("ExternalCommands", "ECCount", 0);
        var num = 0;
        foreach (var addinItem in itemList)
        {
            if (addinItem.Save)
            {
                WriteExternalCommand(file, addinItem, ++num);
            }
        }

        file.Write("ExternalCommands", "ECCount", num);
    }

    private void WriteExternalCommand(IniFile file, AddinItem item, int number)
    {
        file.Write("ExternalCommands", "ECName" + number, item.Name);
        file.Write("ExternalCommands", "ECClassName" + number, item.FullClassName);
        file.Write("ExternalCommands", "ECAssembly" + number, item.AssemblyName);
        file.Write("ExternalCommands", "ECDescription" + number, item.Description);
    }
    
    private List<AddinItem> itemList;

    private string filePath;

    private bool save;

    private bool hidden;
}