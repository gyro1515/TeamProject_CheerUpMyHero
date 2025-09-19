using System.Collections.Generic;

[ExcelAsset(AssetPath = "Resources/DB")]
public class MainStageSO : MonoSO<MainStageData>
{
    public List<MainStageData> MainStageList = new ();

    public override List<MainStageData> GetList()
    {
        return MainStageList;
    }
}
