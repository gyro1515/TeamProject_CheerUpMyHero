using UnityEngine;

public class Skill_GiantCoffin : ActiveSkillEffect
{
    public override void Execute(ActiveArtifactLevelData levelData)
    {
        Debug.Log("스킬 5: 거인의 석관 발동!");

        float duration = levelData.summonDuration;
        float health = levelData.summonHealth;
        float offset = 1f; // 기획서 고정값 (거리 1)

        Vector3 playerPos = GameManager.Instance.Player.transform.position;
        Vector3 summonPos = playerPos + new Vector3(offset, 0, 0);
        PoolType poolTypeToSummon = levelData.summonPoolType;
        if (poolTypeToSummon == PoolType.None)
        {
            Debug.LogWarning($"summonPoolType이 'None'입니다. 'Allies_UnitGolem'으로 강제 설정합니다.");
            // 3. Allies_UnitGolem으로 강제로 바꿔치기합니다.
            poolTypeToSummon = PoolType.Allies_UnitGolem;
        }
        //  PoolType으로 소환수 오브젝트 풀링
        GameObject summon = ObjectPoolManager.Instance.Get(poolTypeToSummon);
        if (summon != null)
        {
            summon.transform.position = summonPos;
        }
        else
        {
            Debug.LogError($"ObjectPoolManager에서 PoolType: {levelData.summonPoolType}을 Get하지 못했습니다.");
        }
        Debug.Log($"위치 {summonPos}에 {duration}초간 {health} 체력의 수호 정령 소환!");
    }
}