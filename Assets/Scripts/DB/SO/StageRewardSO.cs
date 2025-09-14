using System.Collections.Generic;

[ExcelAsset(AssetPath = "Resources/DB")]
public class StageRewardSO : MonoSO<StageRewardData>
{
    public List<StageRewardData> stageRewardList = new List<StageRewardData>();

    public override List<StageRewardData> GetList()
    {
        return stageRewardList;
    }
}
