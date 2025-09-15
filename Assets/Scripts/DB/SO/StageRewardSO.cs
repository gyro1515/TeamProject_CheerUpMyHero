using System.Collections.Generic;

[ExcelAsset(AssetPath = "Resources/DB")]
public class StageRewardSO : MonoSO<StageRewardData>
{
    public List<StageRewardData> StageReward = new List<StageRewardData>();

    public override List<StageRewardData> GetList()
    {
        return StageReward;
    }
}
