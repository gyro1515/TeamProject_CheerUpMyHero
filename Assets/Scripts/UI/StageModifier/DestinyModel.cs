using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestinyModel
{
    private const float NormalBaseProbability = 0.50f;
    private const float NormalMinProbability = 0.32f;
    private const float HardBaseProbability = 0.30f;
    private const float HardMinProbability = 0.12f;

    // 모드, 스테이지로 확률 정하는 메서드
    public float GetFortuneProbability(GameMode mode, (int main, int sub) stage)
    {
        float baseProbability = 0.00f;
        float minProbability = 0.00f;
        int stageNum = stage.main * 9 + stage.sub;

        if (mode == GameMode.Normal)
        {
            baseProbability = NormalBaseProbability;
            minProbability = NormalMinProbability;
        }
        else if (mode == GameMode.Hard)
        {
            baseProbability = HardBaseProbability;
            minProbability = HardMinProbability;
        }

        float penalty = stageNum * 0.02f;
        float finalProbability = baseProbability - penalty;

        return Mathf.Max(finalProbability, minProbability);
    }

    public StageDestinyData GetSpecificDestiny(int destinyID)
    {
        foreach(StageModifierData modifier in DataManager.Instance.StageModifierData.Values)
        {
            if (modifier.idNumber == destinyID && modifier is StageDestinyData destiny)
            {
                return destiny;
            }
        }
        return null;
    }

    // 랜덤 운명 추첨하는 메서드
    public StageDestinyData GetRandomDestiny(DestinyType type)
    {
        List<StageDestinyData> destinyList = new List<StageDestinyData>();
        foreach (StageModifierData modifier in DataManager.Instance.StageModifierData.Values)
        {
            if (modifier is StageDestinyData destiny && destiny.destinyType == type)
            {
                destinyList.Add(destiny);
            }
        }

        if (destinyList.Count > 0)
        {
            int randomIndex = Random.Range(0, destinyList.Count);
            return destinyList[randomIndex];
        }

        return null;
    }

    // 플레이어 데이터 매니저에 운명 넣어주는 메서드
    public void ApplyDestiny(StageDestinyData destiny)
    {
        if (destiny == null)
        {
            Debug.Log("적용할 운명 효과 null임. 운명 관련 로직에 문제 있어요");
            return;
        }
        PlayerDataManager.Instance.currentDestiny = destiny;
        Debug.Log($"{destiny.name} 효과 잘 적용됨");
    }
}
