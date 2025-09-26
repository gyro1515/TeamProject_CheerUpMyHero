using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

[System.Serializable]
public class StatDisplay
{
    public StatType statType;
    public TextMeshProUGUI statText;
}

public class UIArtifactStatArea : MonoBehaviour
{
    [Header("플레이어 스탯 UI")]
    [SerializeField] private List<StatDisplay> _playerStatDisplays;

    [Header("근거리 유닛 스탯 UI")]
    [SerializeField] private List<StatDisplay> _meleeStatDisplays;

    [Header("원거리 유닛 스탯 UI")]
    [SerializeField] private List<StatDisplay> _rangedStatDisplays;

    private void OnEnable()
    {
        PlayerDataManager.Instance.OnEquipArtifactChanged += UpdateAllDisplay;
        UpdateAllDisplay();
    }

    private void OnDisable()
    {
        PlayerDataManager.Instance.OnEquipArtifactChanged -= UpdateAllDisplay;
    }

    private void UpdateAllDisplay()
    {
        UpdateDisplay(EffectTarget.Player, _playerStatDisplays);
        UpdateDisplay(EffectTarget.MeleeUnit, _meleeStatDisplays);
        UpdateDisplay(EffectTarget.RangedUnit, _rangedStatDisplays);
    }

    private void UpdateDisplay(EffectTarget target, List<StatDisplay> statDisplay)
    {
        var bonuseStat = PlayerDataManager.Instance.CalculateArtifactTotalBonusStat(target);

        foreach (var display in statDisplay)
        {
            if (display.statText == null) continue;

            bonuseStat.TryGetValue(display.statType, out float value);

            display.statText.text = $"+ {value}%";
        }
    }
}
