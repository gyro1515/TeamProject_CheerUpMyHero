using System.Collections.Generic;

//[ExcelAsset(AssetPath = "Resources/DB")] 수동으로 임포터 해야해서 주석처리
public class BuildingUpgradeSO : MonoSO<BuildingUpgradeData>
{
    public List<BuildingUpgradeData> BuildingUpgrade = new List<BuildingUpgradeData>();

    public override List<BuildingUpgradeData> GetList()
    {
        return BuildingUpgrade;
    }

    public override void SetData(Dictionary<int, BuildingUpgradeData> DB)
    {
        for (int i = 0; i < BuildingUpgrade.Count; i++)                // 엑셀 데이터 개수만큼 반복문 반복, 데이터 가져와서 딕셔너리에 넣어주는 반복문
        {
            var data = BuildingUpgrade[i];                             // data : 엑셀 데이터 중 i번째 데이터.
            if (data == null) continue;

            // if (DB.ContainsKey(data.idNumber))             // 데이터에서 i번째 데이터의 id 숫자 있는지 검색하고 있으면 에러 메세지
            DB[data.idNumber] = data;                      // 딕셔너리에 key 값을 idNumber로, value 값을 엑셀의 i번째 데이터로 저장함.
        }
    }
}
