using UnityEngine;

public class Skill_GiantCoffin : ActiveSkillEffect
{
    float minX = float.MinValue;
    float maxX = float.MaxValue;
    public override void Execute(ActiveArtifactLevelData levelData)
    {
        Debug.Log("스킬 5: 거인의 석관 발동!");

        float duration = levelData.summonDuration;
        float health = levelData.summonHealth;
        float offset = 1f; // 기획서 고정값 (거리 1)

        if(minX == float.MinValue || maxX == float.MaxValue)
        {
            SetMinMaxX();
        }
        Vector3 playerPos = GameManager.Instance.Player.transform.position;
        Vector3 summonPos = playerPos + new Vector3(offset, 0, 0);
        
        summonPos.x = Mathf.Clamp(summonPos.x, minX, maxX);


        // 현재는 소환 수 고정, 추후 소환수가 달라진다면 아래 내용 사용

        /*PoolType poolTypeToSummon = levelData.summonPoolType;
        if (poolTypeToSummon == PoolType.None)
        {
            Debug.LogWarning($"summonPoolType이 'None'입니다. 'Allies_UnitGolem'으로 강제 설정합니다.");
            // 3. Allies_UnitGolem으로 강제로 바꿔치기합니다.
            poolTypeToSummon = PoolType.Allies_UnitGolem;
        }*/

        //  PoolType으로 소환수 오브젝트 풀링
        GameObject summon = ObjectPoolManager.Instance.Get(PoolType.Allies_UnitGolem);
        if (summon != null)
        {
            summon.transform.position = summonPos;
            GameObject fxGO = ObjectPoolManager.Instance.Get(PoolType.FXActiveAf5);
            summonPos.y += 0.8f;
            fxGO.transform.position = summonPos;
        }
        else
        {
            Debug.LogError($"ObjectPoolManager에서 PoolType: {levelData.summonPoolType}을 Get하지 못했습니다.");
        }
        Debug.Log($"위치 {summonPos}에 {duration}초간 {health} 체력의 수호 정령 소환!");
    }
    void SetMinMaxX()
    {
        BaseHQ playerHQ = GameManager.Instance.PlayerHQ; // 게임 매니저에서 가져와야 함
        BaseHQ enemyHQ = GameManager.Instance.enemyHQ;

        minX = playerHQ.gameObject.transform.position.x;
        maxX = enemyHQ.gameObject.transform.position.x;
    }
}