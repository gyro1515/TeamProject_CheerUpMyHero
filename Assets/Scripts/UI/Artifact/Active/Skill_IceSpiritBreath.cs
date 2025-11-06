using UnityEngine;
using System.Collections.Generic;
using System.Linq; 

public class Skill_IceSpiritBreath : ActiveSkillEffect
{
    public override void Execute(ActiveArtifactLevelData levelData)
    {
        Debug.Log("스킬 1: 얼음 정령의 숨결 발동!");

        // 기획서 이미지의 고정값
        float duration = 15f;
        float range = 10f;
        float slowPercent = 15f;
        float atkCooldownPercent = 15f;
        // 데이터 시트에서 가져오는 값
        float damage = (GameManager.Instance.Player.AtkPower) * levelData.damageBonusPercent / 100f; // 예시 피해량
        Debug.Log(damage);
        float playerX = GameManager.Instance.Player.transform.position.x;
        GameObject fxGO = ObjectPoolManager.Instance.Get(PoolType.FXActiveAf1);
        Vector3 fxSpawnPos = GameManager.Instance.Player.transform.position;
        fxSpawnPos.y += 1.4f;
        fxGO.transform.position = fxSpawnPos;
        List<BaseCharacter> enemies = UnitManager.EnemyUnitList;
        foreach (var enemy in enemies.ToList())
        {
            if (enemy == null || enemy.IsDead) continue;
            if (enemy.GetComponent<EnemyHQ>() != null)
            {
                continue; 
            }
            if (enemy.transform.position.x > playerX && enemy.transform.position.x <= playerX + range)
            {
                if (damage > 0) enemy.GetComponent<IDamageable>()?.TakeDamage(damage);

                var buffController = enemy.GetComponent<BuffController>();
                if (buffController != null)
                {
                    buffController.ApplyDebuff(DebuffType.MoveSpeed, duration, slowPercent);
                    buffController.ApplyDebuff(DebuffType.AttackCooldown, duration, atkCooldownPercent);
                    buffController.ChangeColor(Color.blue, duration); // 파란색으로 변경
                }
            }
        }
    }
}