using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ActiveArtifactData : ArtifactData
{
    public string type;
    public int cost;
    public List<ActiveArtifactLevelData> levelData;
    public int curLevel;
}