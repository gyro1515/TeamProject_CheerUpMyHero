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
        GameObject fxGO = ObjectPoolManager.Instance.Get(PoolType.FXActiveAf3);
        fxGO.transform.position = GameManager.Instance.Player.transform.position;
        List<BaseCharacter> allies = UnitManager.PlayerUnitList;
        foreach (var ally in allies.ToList())
        {
            if (ally == null || ally.IsDead) continue;
            var buffController = ally.GetComponent<BuffController>();

            if (buffController != null)
            {
                buffController.ApplyBuff(BuffType.AttackDamage, duration, atkPercent);
                buffController.ApplyBuff(BuffType.AttackSpeed, duration, atkSpeedPercent);
            }
        }
    }
}