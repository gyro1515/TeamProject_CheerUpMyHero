using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIRandomArtifactSlot : BaseUI
{
    [Header("유물 정보")]
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private TextMeshProUGUI _descriptionText;

    [Header("테두리")]
    [SerializeField] private Outline _iconOutline;
    [SerializeField] private Outline _textBgOutline;

    [Header("등급별 테두리 색상")]
    [SerializeField] private Color _commonBorder = Color.gray;
    [SerializeField] private Color _rareBorder = Color.blue;
    [SerializeField] private Color _epicBorder = Color.magenta;
    [SerializeField] private Color _uniqueBorder = Color.yellow;
    [SerializeField] private Color _legendaryBorder = Color.green;

    public event Action<ArtifactData> OnStageClearArtifactSlotClicked;

    private ArtifactData _data;
    private Button _button;

    private void Awake()
    {
        _button = GetComponent<Button>();
        _button.onClick.AddListener(OnButtonClicked);
    }

    public void Init(ArtifactData data)
    {
        _data = data;
        UpdateSlot();
    }

    private void UpdateSlot()
    {
        if (_data == null)
        {
            gameObject.SetActive(false);
            return;
        }

        gameObject.SetActive(true);
        _nameText.text = _data.name;
        _descriptionText.text = _data.description;

        // 유물 이미지 넣는 로직 추가해야 함.

        if (_data is PassiveArtifactData passiveData)
        {
            #region 테두리 효과주기
            switch (passiveData.grade)
            {
                case PassiveArtifactGrade.Common:
                    _iconOutline.effectColor = _commonBorder;
                    _textBgOutline.effectColor = _commonBorder;
                    break;

                case PassiveArtifactGrade.Rare:
                    _iconOutline.effectColor = _rareBorder;
                    _textBgOutline.effectColor = _rareBorder;
                    break;

                case PassiveArtifactGrade.Epic:
                    _iconOutline.effectColor = _epicBorder;
                    _textBgOutline.effectColor = _epicBorder;
                    break;

                case PassiveArtifactGrade.Unique:
                    _iconOutline.effectColor = _uniqueBorder;
                    _textBgOutline.effectColor = _uniqueBorder;
                    break;

                case PassiveArtifactGrade.Legendary:
                    _iconOutline.effectColor = _legendaryBorder;
                    _textBgOutline.effectColor = _legendaryBorder;
                    break;

                default:
                    _iconOutline.effectColor = Color.black;
                    _textBgOutline.effectColor = Color.black;
                    break;
            }
            #endregion
        }
        else if (_data is ActiveArtifactData activeData)
        {
            _iconOutline.effectColor = _legendaryBorder;
            _textBgOutline.effectColor = _legendaryBorder;
        }
    }

    private void OnButtonClicked()
    {
        OnStageClearArtifactSlotClicked?.Invoke(_data);
    }
}
