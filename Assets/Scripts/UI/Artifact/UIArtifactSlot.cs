using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIArtifactSlot : MonoBehaviour
{
    [Header("유물 데이터 적용")]
    [SerializeField] private Image _artifactIcon;
    [SerializeField] private Image _borderImage;
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private TextMeshProUGUI _statTypeText;
    [SerializeField] private TextMeshProUGUI _statValueText;

    [Header("등급별 테두리 색상")]
    [SerializeField] private Color _commonBorder = Color.gray;
    [SerializeField] private Color _rareBorder = Color.blue;
    [SerializeField] private Color _epicBorder = Color.magenta;
    [SerializeField] private Color _uniqueBorder = Color.yellow;
    [SerializeField] private Color _legendaryBorder = Color.green;

    private PassiveArtifactData _data;

    private void Init()
    {
        
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

        // 지금 유물 데이터에 아이콘 스프라이트가 없어용
        // 슬롯에 출력될 유물 아이콘 필요해요
        
        if (_data is PassiveArtifactData passiveArtifact)
        {
            #region 테두리 색깔 결정하기
            switch (passiveArtifact.grade)
            {
                case PassiveArtifactGrade.Common:
                    _borderImage.color = _commonBorder;
                    break;

                case PassiveArtifactGrade.Rare:
                    _borderImage.color = _rareBorder;
                    break;
                
                case PassiveArtifactGrade.Epic:
                    _borderImage.color= _epicBorder;
                    break;

                case PassiveArtifactGrade.Unique:
                    _borderImage.color = _uniqueBorder;
                    break;

                case PassiveArtifactGrade.Legendary:
                    _borderImage.color =_legendaryBorder;
                    break;

                default:
                    _borderImage.color = _commonBorder;
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
}
