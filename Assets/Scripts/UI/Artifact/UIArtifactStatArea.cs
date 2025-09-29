using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class StatDisplay
{
    public StatType statType;
    public TextMeshProUGUI statText;
}

public class UIArtifactStatArea : BaseUI
{
    [Header("플레이어 스탯 UI")]
    [SerializeField] private Image _playerHpBonusBar;
    [SerializeField] private Image _playerAtkBonusBar;
    [SerializeField] private Image _playerSpdBonusBar;

    private float _playerHpBonus;
    private float _playerHpBonusMax;
    private const int _playerHpLegendary = 80200025;

    private float _playerAtkBonus;
    private float _playerAtkBonusMax;
    private const int _playerAtkLegendary = 80200015;

    private float _playerSpdBonus;
    private float _playerSpdBonusMax;
    private const int _playerSpdLegendary = 80200035;

    // 근거리 원거리 유닛 로직 추가해야 함

    private const float LegendaryArtifactValue = 25f;

    private void OnEnable()
    {
        UpdateStatUI();
        ArtifactManager.Instance.OnEquippedArtifactChanged += UpdateStatUI;
    }

    private void OnDisable()
    {
        ArtifactManager.Instance.OnEquippedArtifactChanged -= UpdateStatUI;
    }

    private void UpdateStatUI()     // ㅋㅋ 이게 최선인가.... 개선해야 할 듯
    {
        _playerHpBonus = ArtifactManager.Instance.GetPassiveArtifactStatBonus(EffectTarget.Player, StatType.MaxHp);
        _playerHpBonusMax = ArtifactManager.Instance.GetPassiveArtifactDataValue(_playerHpLegendary);
        _playerHpBonusBar.fillAmount = _playerHpBonus / _playerHpBonusMax;

        _playerAtkBonus = ArtifactManager.Instance.GetPassiveArtifactStatBonus(EffectTarget.Player, StatType.AtkPower);
        _playerAtkBonusMax = ArtifactManager.Instance.GetPassiveArtifactDataValue(_playerAtkLegendary);
        _playerAtkBonusBar.fillAmount = _playerAtkBonus / _playerAtkBonusMax;

        _playerSpdBonus = ArtifactManager.Instance.GetPassiveArtifactStatBonus(EffectTarget.Player, StatType.MoveSpeed);
        _playerSpdBonusMax = ArtifactManager.Instance.GetPassiveArtifactDataValue(_playerSpdLegendary);
        _playerSpdBonusBar.fillAmount = _playerSpdBonus / _playerSpdBonusMax;

        // 로직 추가되면 스탯 추가해야 함
    }
}
