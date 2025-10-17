using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExcelAsset(AssetPath = "Resources/DB")]
public class StageModifierSO : MonoSO<StageModifierData>
{
	public List<StageDestinyData> DestinyEffects;
	public List<StageDestinyModifier> DestinyEffectModifier;
	public List<StageChallengeData> ChallengeEffects;

    public override List<StageModifierData> GetList()
    {
        return null;
    }

    public override void SetData(Dictionary<int, StageModifierData> DB)
    {
        for (int i = 0; i < DestinyEffects.Count; i++)
        {
            var data = DestinyEffects[i];
            if (data == null) continue;

            DB[data.idNumber] = data;
        }

        for (int i = 0; i < ChallengeEffects.Count; i++)
        {
            var data = ChallengeEffects[i];
            if (data == null) continue;

            DB[data.idNumber] = data;
        }

        for (int i = 0; i < DestinyEffectModifier.Count; i++)
        {
            var data = DestinyEffectModifier[i];
            if (data == null) continue;
            if (DB[data.idNumber] is StageDestinyData destiny)
            {
                destiny.modifiers.Add(data);
            }
        }
    }
}
