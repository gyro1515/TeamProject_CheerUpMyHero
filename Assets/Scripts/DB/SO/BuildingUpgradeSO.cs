using System.Collections.Generic;

//[ExcelAsset(AssetPath = "Resources/DB")] 수동으로 임포터 해야해서 주석처리
public class BuildingUpgradeSO : MonoSO<BuildingUpgradeData>
{
    public List<BuildingUpgradeData> BuildingUpgrade = new List<BuildingUpgradeData>();

    public override List<BuildingUpgradeData> GetList()
    {
        return BuildingUpgrade;
    }
}
