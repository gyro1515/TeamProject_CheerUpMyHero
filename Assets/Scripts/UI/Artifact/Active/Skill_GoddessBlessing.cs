using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Skill_GoddessBlessing : ActiveSkillEffect
{
    public override void Execute(ActiveArtifactLevelData levelData)
    {
        Debug.Log("스킬 4: 여신의 축복 발동! (3초 후)");
        ArtifactManager.Instance.StartCoroutine(Co_GoddessBlessing(levelData));
    }

    private IEnumerator Co_GoddessBlessing(ActiveArtifactLevelData levelData)
    {
        float delay = 3f; // 기획서 고정값
        float healPercent = levelData.healPercent;

        yield return new WaitForSeconds(delay);
        GameObject fxGO = ObjectPoolManager.Instance.Get(PoolType.FXActiveAf4);
        Vector3 fxSpawnPos = GameManager.Instance.Player.transform.position;
        fxSpawnPos.y += 1.4f;
        fxGO.transform.position = fxSpawnPos; List<BaseCharacter> allies = UnitManager.PlayerUnitList;
        foreach (var ally in allies.ToList())
        {
            if (ally == null || ally.IsDead) continue;
            float healAmount = ally.MaxHp * healPercent / 100f; // 최대 체력 비례
            ally.GetComponent<BaseController>()?.TakeHeal(healAmount);
        }
    }
}