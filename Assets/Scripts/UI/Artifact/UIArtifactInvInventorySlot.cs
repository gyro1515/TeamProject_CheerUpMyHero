using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System;

public class UIArtifactInvInventorySlot : BaseUI
{
    [Header("유물 데이터 적용")]
    [SerializeField] private Image _artifactIcon;
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private TextMeshProUGUI _statTypeText;
    [SerializeField] private TextMeshProUGUI _statValueText;
    [SerializeField] private GameObject _equippedImage;

    [Header("등급별 테두리 색상")]
    [SerializeField] private Color _commonBorder = Color.gray;
    [SerializeField] private Color _rareBorder = Color.blue;
    [SerializeField] private Color _epicBorder = Color.magenta;
    [SerializeField] private Color _uniqueBorder = Color.yellow;
    [SerializeField] private Color _legendaryBorder = Color.green;

    private Outline _outline;
    private PassiveArtifactData _data;
    private Button _button;

    public event Action<PassiveArtifactData> OnArtifactInventorySlotClicked;

    private void Awake()
    {
        _outline = GetComponent<Outline>();

        _button = GetComponent<Button>();
        _button.onClick.AddListener(OnButtonClicked);
    }

    public void Init(PassiveArtifactData data, bool isEquipedThisSlot)
    {
        _data = data;
        _equippedImage.SetActive(isEquipedThisSlot);
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (_data == null)
        {
            gameObject.SetActive(false);
            return;
        }

        gameObject.SetActive(true);
        _nameText.text = _data.name;

        // 나중에 유물 이미지 넣는 로직 추가해야 함
        
        if (_data is PassiveArtifactData passiveArtifact)
        {
            #region 테두리 색깔 결정하기
            switch (passiveArtifact.grade)
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
                    _outline.effectColor =_legendaryBorder;
                    break;

                default:
                    _outline.effectColor = Color.black;
                    break;
            }
            #endregion

            #region 스탯 타입 출력하기
            switch (passiveArtifact.statType)
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

            _statValueText.text = passiveArtifact.value.ToString();
        }
    }

    private void OnButtonClicked()
    {
        OnArtifactInventorySlotClicked?.Invoke(_data);
    }
}
