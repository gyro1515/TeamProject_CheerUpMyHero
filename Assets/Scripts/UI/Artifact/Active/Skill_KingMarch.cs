using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class Skill_KingMarch : ActiveSkillEffect
{
    float duration = 10f;
    // 미리 준비해 둘 버프 효과들
    BuffStatPercent damageBuffPercent;
    BuffStatPercent attackSpeedBuffPercent;
    List<BuffEffect> buffEffects;
    public Skill_KingMarch() : base()
    {
        // 수치는 스킬 사용할 때 데이터 시트에서 가져와서 적용 -> Skill_IceSpiritBreath과 다름
        damageBuffPercent = new BuffStatPercent(IntegratedBuffType.AttackPower, 0);
        attackSpeedBuffPercent = new BuffStatPercent(IntegratedBuffType.AttackRate, 0);
        buffEffects = new List<BuffEffect> { damageBuffPercent, attackSpeedBuffPercent };
    }
    public override void Execute(ActiveArtifactLevelData levelData)
    {
        Debug.Log("스킬 3: 왕국의 진군가 발동!");

        float atkPercent = levelData.attackBonusPercent;
        float atkSpeedPercent = levelData.attackSpeedBonusPercent;
        GameObject fxGO = ObjectPoolManager.Instance.Get(PoolType.FXActiveAf3);
        fxGO.transform.position = GameManager.Instance.Player.transform.position;
        List<BaseCharacter> allies = UnitManager.PlayerUnitList;
        // 버프 수치 업데이트
        damageBuffPercent.ChagePercentValue(atkPercent / 100f);
        //attackSpeedBuffPercent.ChagePercentValue(-atkSpeedPercent / 100f);
        attackSpeedBuffPercent.ChagePercentValue(-0.5f);
        foreach (var ally in allies)
        {
            if (ally == null || ally.IsDead) continue;
            var buffController = ally.GetComponent<BuffController>();

            if (buffController != null)
            {
                // 이전 코드
                /*buffController.ApplyBuff(BuffType.AttackDamage, duration, atkPercent);
                buffController.ApplyBuff(BuffType.AttackSpeed, duration, atkSpeedPercent);*/
                // 리팩토링 코드
                buffController.ApplyBuff(BuffSource.Skill_KingMarch, buffEffects, duration);
            }
        }
    }
}