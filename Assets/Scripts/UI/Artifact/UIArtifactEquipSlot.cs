using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIArtifactEquipSlot : MonoBehaviour
{
    [Header("유물 데이터 적용")]
    [SerializeField] private Image _artifactIcon;
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private TextMeshProUGUI _statTypeText;
    [SerializeField] private TextMeshProUGUI _statValueText;

    [Header("등급별 테두리 색상")]
    [SerializeField] private Color _commonBorder = Color.gray;
    [SerializeField] private Color _rareBorder = Color.blue;
    [SerializeField] private Color _epicBorder = Color.magenta;
    [SerializeField] private Color _uniqueBorder = Color.yellow;
    [SerializeField] private Color _legendaryBorder = Color.green;

    private Button _button;
    private Outline _outline;

    private EffectTarget _target;
    private int _slotIndex;

    public void Init(EffectTarget target, int slotIndex, UIPassiveArtifactInventory inventory)
    {
        _target = target;
        _slotIndex = slotIndex;
        
        _outline = GetComponent<Outline>();
        _button = GetComponent<Button>();
        _button.onClick.AddListener(() => inventory.OpenInventory(_target, _slotIndex));

        PlayerDataManager.Instance.OnEquipArtifactChanged += UpdateUI;
        UpdateUI();
    }

    private void UpdateUI()
    {
        PassiveArtifactData equippedArtifact = PlayerDataManager.Instance.EquippedArtifacts[_target][_slotIndex];

        if (equippedArtifact != null)
        {
            // 유물 아이콘 넣기
            _nameText.text = equippedArtifact.name;

            #region 테두리 색깔 결정하기
            switch (equippedArtifact.grade)
            {
                case PassiveArtifactGrade.Common:
                    _outline.effectColor = _commonBorder;
                    break;

                case PassiveArtifactGrade.Rare:
                    _outline.effectColor = _rareBorder;
                    break;

                case PassiveArtifactGrade.Epic:
                    _outline.effectColor = _epicBorder;
                    break;

                case PassiveArtifactGrade.Unique:
                    _outline.effectColor = _uniqueBorder;
                    break;

                case PassiveArtifactGrade.Legendary:
                    _outline.effectColor = _legendaryBorder;
                    break;

                default:
                    _outline.effectColor = Color.black;
                    break;
            }
            #endregion

            #region 스탯 타입 출력하기
            switch (equippedArtifact.statType)
            {
                case StatType.MaxHp:
                    _statTypeText.text = "HP";
                    break;

                case StatType.AtkPower:
                    _statTypeText.text = "ATK";
                    break;

                case StatType.MoveSpeed:
                    _statTypeText.text = "SPD";
                    break;

                case StatType.AuraRange:
                    _statTypeText.text = "RNG";
                    break;

                default:
                    _statTypeText.text = "ERROR";
                    break;
            }
            #endregion

            _statValueText.text = equippedArtifact.value.ToString();
        }
    }

    private void OnDisable()
    {
        PlayerDataManager.Instance.OnEquipArtifactChanged -= UpdateUI;
    }
}
