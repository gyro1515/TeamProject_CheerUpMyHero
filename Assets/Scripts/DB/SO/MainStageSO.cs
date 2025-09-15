using System.Collections.Generic;

[ExcelAsset(AssetPath = "Resources/DB")]
public class MainStageSO : MonoSO<MainStageData>
{
    public List<MainStageData> StageReward = new List<MainStageData>();

    public override List<MainStageData> GetList()
    {
        return StageReward;
    }
}
