using UnityEngine;
using System.Collections.Generic;
using System.Linq; 

public class Skill_IceSpiritBreath : ActiveSkillEffect
{
    // 기획서 이미지의 고정값
    float duration = 15f;
    float range = 10f;
    float slowPercent = 0.15f;
    float atkCooldownPercent = 0.15f;
    // 미리 준비해 둘 버프 효과들
    List<BuffEffect> buffEffects;
    BuffColorType debuffColorType = BuffColorType.Blue;
    public Skill_IceSpiritBreath() : base()
    {
        /*// 기획서 이미지의 고정값
        duration = 15f;
        range = 10f;
        slowPercent = 15f;
        atkCooldownPercent = 15f;*/
        // 버프 이펙트 배열 만들기
        // Skill_KingMarch과 다르게 이 스킬은 버프 수치 고정
        BuffStatPercent slowEffect = new BuffStatPercent(IntegratedBuffType.MoveSpeed, -slowPercent);
        BuffStatPercent atkCooldownEffect = new BuffStatPercent(IntegratedBuffType.AttackRate, atkCooldownPercent);
        buffEffects = new List<BuffEffect> { slowEffect, atkCooldownEffect };
    }
    public override void Execute(ActiveArtifactLevelData levelData)
    {
        Debug.Log("스킬 1: 얼음 정령의 숨결 발동!");
        // 데이터 시트에서 가져오는 값
        float damage = (GameManager.Instance.Player.FinAttackPower) * levelData.damageBonusPercent / 100f; // 예시 피해량
        //Debug.Log(damage);
        // 테스트로 데미지 1, 지속시간 3
        /*float damage = 1f;
        duration = 3f;*/
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
                    // 이전 코드
                    /*buffController.ApplyDebuff(DebuffType.MoveSpeed, duration, slowPercent);
                    buffController.ApplyDebuff(DebuffType.AttackCooldown, duration, atkCooldownPercent);
                    buffController.ChangeColor(Color.blue, duration); // 파란색으로 변경*/
                    // 리팩토링 코드
                    buffController.ApplyBuff(BuffSource.Skill_IceSpiritBreath, buffEffects, duration);
                    buffController.ApplyBuffColor(debuffColorType, duration); // 파란색으로 변경
                }
            }
        }
    }
}