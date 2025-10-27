using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class Skill_KingMarch : ActiveSkillEffect
{
    public override void Execute(ActiveArtifactLevelData levelData)
    {
        Debug.Log("스킬 3: 왕국의 진군가 발동!");

        float duration = 10f; 
        float atkPercent = levelData.attackBonusPercent;
        float atkSpeedPercent = levelData.attackSpeedBonusPercent;

        List<BaseCharacter> allies = UnitManager.Instance.PlayerUnitList;
        foreach (var ally in allies.ToList())
        {
            if (ally == null || ally.IsDead) continue;
            var controller = ally.GetComponent<BaseController>();
            if (controller != null)
            {
                controller.ApplyBuff(BuffType.AttackDamage, duration, atkPercent);
                controller.ApplyBuff(BuffType.AttackSpeed, duration, atkSpeedPercent);
            }
        }
    }
}