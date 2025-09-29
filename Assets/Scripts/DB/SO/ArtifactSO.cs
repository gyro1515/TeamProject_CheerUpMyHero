using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExcelAsset(AssetPath = "Resources/DB")]
public class ArtifactSO : MonoSO<ArtifactData>
{
    public List<ActiveArtifactData> activeArtifacts;
    public List<PassiveArtifactData> passiveArtifacts;
    public List<ActiveArtifactLevelData> activeArtifactLevels;

    public override List<ArtifactData> GetList()
    {
        return null;
    }

    public override void SetData(Dictionary<int, ArtifactData> DB)
    {
        for (int i = 0; i < activeArtifacts.Count; i++)
        {
            var data = activeArtifacts[i];
            if (data == null) continue;

            DB[data.idNumber] = data;
        }

        for (int i = 0; i < passiveArtifacts.Count; i++)
        {
            var data = passiveArtifacts[i];
            if (data == null) continue;

            // data.ArtifactGradeProcess();

            DB[data.idNumber] = data;
        }

        for (int i = 0; i < activeArtifactLevels.Count; i++)
        {
            var data = activeArtifactLevels[i];
            if (data == null) continue;
            if (DB[data.idNumber] is ActiveArtifactData activeAf)
            {
                activeAf.levelData.Add(data);

                // activeArtifacts.Find(x => x == activeAf).levelData.Add(data);
            }
            

        }
    }

}
