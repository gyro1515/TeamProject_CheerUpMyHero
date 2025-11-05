using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Skill_ThunderJudgment : ActiveSkillEffect
{
    public override void Execute(ActiveArtifactLevelData levelData)
    {
        Debug.Log("스킬 2: 천동설의 심판 발동!");
        ArtifactManager.Instance.StartCoroutine(Co_ThunderJudgment(levelData));
    }

    private IEnumerator Co_ThunderJudgment(ActiveArtifactLevelData levelData)
    {
        int hitCount = 5; // 기획서 고정값
        float interval = 1f; // 기획서 고정값
        float damage = (GameManager.Instance.Player.AtkPower) * levelData.damageBonusPercent / 100f;

        for (int i = 0; i < hitCount; i++)
        {
            List<BaseCharacter> enemies = UnitManager.EnemyUnitList;
            foreach (var enemy in enemies.ToList())
            {
                if (enemy == null || enemy.IsDead) continue;
                if (enemy.GetComponent<EnemyHQ>() != null)
                {
                    continue; // HQ는 번개 건너뛰기
                }
                if (damage > 0) enemy.GetComponent<IDamageable>()?.TakeDamage(damage);
                Debug.Log(damage);
            }
            GameObject fxGO = ObjectPoolManager.Instance.Get(PoolType.FXActiveAf2);
            Vector3 fxSpawnPos = GameManager.Instance.Player.transform.position;
            fxSpawnPos.y += 1.4f;
            fxGO.transform.position = fxSpawnPos;
            yield return new WaitForSeconds(interval);
        }
    }
}