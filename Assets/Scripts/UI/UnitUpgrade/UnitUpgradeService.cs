using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitUpgradeService
{
    private readonly PlayerDataManager _data;

    // 유닛 카드 업그레이드 조건이 뭐가 될 지 몰라서 const로 일단 만들어둠
    private const int UpgradeRequiredCount = 3;

    public UnitUpgradeService(PlayerDataManager data)
    {
        _data = data;
    }

    #region 조회 메서드
    public bool CanUpgradeCard(int id)
    {
        if (!_data.OwnedCardData.ContainsKey(id))
        {
            return false;
        }

        int currentCount = GetCardCount(id);
        int requiredCount = GetRequiredCount(id);

        if (currentCount < requiredCount)
        {
            return false;
        }

        // 다른 조건들 쭈우우욱 기술하기
        // ex) 합성 가능 최대 레벨? 등급? 체크하기
        // ex) 합성에 필요한 재화 다 충분히 있는 지 체크하기

        return true;
    }

    public int GetCardCount(int id)
    {
        if (_data.OwnedCardData.TryGetValue(id, out BaseUnitData card))
        {
            return card.ownedCount;
        }
        return 0;
    }

    // 유닛 업그레이드 요구 사항이 어떻게 될 지 몰라서 메서드 분리해둠.
    // 유닛 레벨??에 따라, 등급?? 희귀도??에 따라 다르게 산출 가능하도록 만들어둠
    public int GetRequiredCount(int id)
    {
        // 유닛 등급이나 레벨마다 요구 조건 다르게 하는 코드 쭈우우욱 두기

        return UpgradeRequiredCount;
    }
    #endregion

    #region 합성 메서드
    public async UniTask<bool> UpgradeCard(int id)
    {
        int currentCount = GetCardCount(id);
        int requiredCount = GetRequiredCount(id);

        if (!CanUpgradeCard(id))
        {
            return false;
        }

        // 재화 차감하는 코드 들어가야 함.

        _data.OwnedCardData[id].ownedCount -= requiredCount;
        int afterCount = _data.OwnedCardData[id].ownedCount; // 업그레이드 후에 남은 카드 개수 -> UI 표시용

        _data.CardCountChanged(id, afterCount);

        // 유닛 스탯 증가 or 레벨 증가 or 등급 증가 로직

        await _data.SaveDataToCloudAsync();

        return true;
    }
    #endregion
}
