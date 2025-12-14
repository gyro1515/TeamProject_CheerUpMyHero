using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArtifactUpgradeService
{
    private readonly PlayerDataManager _data;
    private readonly ArtifactService _service;

    public ArtifactUpgradeService(PlayerDataManager data, ArtifactService service)
    {
        _data = data;
        _service = service;
    }

    #region 패시브 유물 합성
    // 패시브 합성 가능 여부 산출 메서드
    public bool CanUpgradePassive(int idNumber)
    {
        if (!DataManager.ArtifactData.TryGetValue(idNumber, out var data))
            return false;

        if (data is not PassiveArtifactData passive)
            return false;

        if (GetSameArtifactCount(idNumber) < 3)
            return false;

        if (GetNextPassiveArtifact(passive) == null)
            return false;

        return true;
    }

    // 같은 유물 몇 개 있는 지 확인하는 메서드
    public int GetSameArtifactCount(int idNumber)
    {
        int count = 0;

        for (int i = 0; i < _data.OwnedArtifacts.Count; i++)
        {
            if (_data.OwnedArtifacts[i].idNumber == idNumber)
            {
                count++;
            }
        }

        return count;
    }

    // 다음 유물 찾는 메서드
    public PassiveArtifactData GetNextPassiveArtifact(PassiveArtifactData source)
    {
        if (source == null) return null;
        if (source.grade == PassiveArtifactGrade.Legendary) return null;

        PassiveArtifactGrade nextGrade = source.grade + 1;

        foreach (ArtifactData artifact in DataManager.ArtifactData.Values)
        {
            if (artifact is PassiveArtifactData passive)
            {
                if (passive.effectTarget == source.effectTarget &&
                    passive.statType == source.statType &&
                    passive.grade == nextGrade)
                {
                    return passive;
                }

            }
        }

        return null;
    }

    // 패시브 유물 합성 메서드 : 기존 유물 세 개 삭제하고 다음 단계 유물 하나 Add.
    // 비동기 쓴 이유 : 서버랑 통신해서 유물 현황 Save해줘야 함 -> 패시브 유물 합성에는 재화 변화 없어서
    public async UniTask<bool> UpgradePassive(int idNumber)
    {
        if (!CanUpgradePassive(idNumber))
            return false;

        var source = DataManager.ArtifactData.GetData(idNumber) as PassiveArtifactData;
        var result = GetNextPassiveArtifact(source);

        int removedCount = _service.RemoveArtifactsByIdNumber(idNumber, 3);

        if (removedCount < 3)
        {
            return false;
        }

        _data.AddOwnedArtifact(result);

        await _data.SaveDataToCloudAsync();

        return true;
    }
    #endregion

    #region 액티브 유물 강화
    // 액티브 강화 가능한 지 체크하는 메서드
    public bool CanUpgradeActive(ActiveArtifactData artifact)
    {
        if (artifact  == null) return false;
        if (artifact.levelData == null ||  artifact.levelData.Count == 0) return false;

        int maxLevel = artifact.levelData.Count - 1;
        if (artifact.curLevel >= maxLevel) return false;

        var cost = GetActiveUpgradeCost(artifact);
        foreach (var pair in cost)
        {
            if (_data.GetResourceAmount(pair.Key) < pair.Value)
                return false;
        }

        return true;
    }

    // 액티브 강화용 코스트 가져오는 메서드
    public Dictionary<ResourceType, int> GetActiveUpgradeCost(ActiveArtifactData artifact)
    {
        var cost = new Dictionary<ResourceType, int>();

        if (artifact == null || artifact.levelData == null)
            return cost;

        int maxLevel = artifact.levelData.Count - 1;
        if (artifact.curLevel >= maxLevel)
            return cost;

        var levelData = artifact.levelData[artifact.curLevel];

        if (levelData.goldCost > 0)
            cost[ResourceType.Gold] = levelData.goldCost;
        if (levelData.woodCost > 0)
            cost[ResourceType.Wood] = levelData.woodCost;
        if (levelData.ironCost > 0)
            cost[ResourceType.Iron] = levelData.ironCost;
        if (levelData.magicStoneCost > 0)
            cost[ResourceType.MagicStone] = levelData.magicStoneCost;

        return cost;
    }

    // 자원량 보고 강화 가능한 지 체크하는 메서드
    public Dictionary<ResourceType, bool> GetResourceSufficiency(ActiveArtifactData artifact)
    {
        var result = new Dictionary<ResourceType, bool>();
        var cost = GetActiveUpgradeCost(artifact);

        foreach (var pair in cost)
        {
            result[pair.Key] = _data.GetResourceAmount(pair.Key) >= pair.Value;
        }

        return result;
    }

    // 액티브 강화하는 메서드 : curLevel 올리고 자원 -해줌.
    // 비동기 쓴 이유 : 자원값 변경하는 메서드가 서버랑 통신해서 비동기로 처리해야 함.
    public async UniTask<bool> UpgradeActive(ActiveArtifactData artifact)
    {
        if (!CanUpgradeActive(artifact))
            return false;

        var cost = GetActiveUpgradeCost(artifact);

        foreach (var pair in cost)
        {
            await _data.AddResource(pair.Key, -pair.Value);
        }

        artifact.curLevel++;

        await _data.SaveDataToCloudAsync();

        return true;
    }
    #endregion
}
