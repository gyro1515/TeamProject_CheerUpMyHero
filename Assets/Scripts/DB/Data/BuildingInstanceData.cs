using System;

[System.Serializable]
public class BuildingInstanceData
{
    public int blueprintId; // 어떤 설계도(BuildingUpgradeData)로 지어졌는지 ID
    public DateTime CooldownEndTime; // 쿨타임이 끝나는 실제 시각

    public BuildingUpgradeData BlueprintData => DataManager.Instance.BuildingUpgradeData.GetData(blueprintId);

    public BuildingInstanceData(int id)
    {
        blueprintId = id;
        CooldownEndTime = DateTime.MinValue; // 쿨타임 없는 상태로 시작
    }
}