using System.Collections.Generic;

[ExcelAsset(AssetPath = "Resources/DB")]
public class SubStageSO : MonoSO<SubStageData>
{
    public List<SubStageData> SubStageList = new();

    public override List<SubStageData> GetList()
    {
        return SubStageList;
    }
}
