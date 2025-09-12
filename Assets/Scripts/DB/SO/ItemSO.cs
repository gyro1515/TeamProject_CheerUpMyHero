using System.Collections.Generic;

[ExcelAsset(AssetPath = "Resources/DB")]
public class ItemSO : MonoSO<ItemData>
{
    public List<ItemData> itemList = new List<ItemData>();
    public override List<ItemData> GetList()
    {
        return itemList;
    }
}
