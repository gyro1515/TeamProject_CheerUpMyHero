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

        //  PoolType으로 소환수 오브젝트 풀링
        // GameObject summon = ObjectPoolManager.Instance.Get(levelData.summonPoolType, summonPos);
        // var summonController = summon.GetComponent<SummonedUnitController>();
        // summonController.Setup(health, duration); 

        Debug.Log($"위치 {summonPos}에 {duration}초간 {health} 체력의 수호 정령 소환!");
    }
}