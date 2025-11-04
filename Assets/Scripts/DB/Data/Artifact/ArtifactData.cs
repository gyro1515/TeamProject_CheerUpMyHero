using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ArtifactData : MonoData
{
    public string name;
    public string description;
    public ArtifactType artifactType;
    public string iconSpritePath;
    public string descriptionForGuide;
}
