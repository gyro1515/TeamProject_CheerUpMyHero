using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(BuildingUpgradeSO))]
public class BuildingUpgradeSOEditor : Editor
{
    public override void OnInspectorGUI()
    {
        GUI.enabled = false;

        base.OnInspectorGUI();

        GUI.enabled = true;
    }
}