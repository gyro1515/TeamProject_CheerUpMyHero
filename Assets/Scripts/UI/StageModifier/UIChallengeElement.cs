using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIChallengeElement : MonoBehaviour
{
    [Header("UI 참조")]
    [SerializeField] private TextMeshProUGUI _name;
    [SerializeField] private TextMeshProUGUI _effectValue;
    [SerializeField] private TextMeshProUGUI _points;
    [SerializeField] private TextMeshProUGUI _level;
    [SerializeField] private Button _plusButton;
    [SerializeField] private Button _minusButton;
    [SerializeField] private Image _challengeIcon;
    [SerializeField] private Image _statIcon;

    private StageChallengeData _challengeData;
    private int _curLv = 0;

    public event Action<int, int> OnElementsLevelChanged;

    public void SetElements(StageChallengeData data)
    {
        _challengeData = data;
        _name.text = _challengeData.name;
        
        _plusButton.onClick.AddListener(OnPlusButtonClicked);
        _minusButton.onClick.AddListener(OnMinusButtonClicked);

        RefreshUI();
    }

    private void OnPlusButtonClicked()
    {
        if (_curLv < _challengeData.maxLevel)
        {
            _curLv++;
            OnElementsLevelChanged?.Invoke(_challengeData.idNumber, _curLv);
            RefreshUI();
        }
    }

    private void OnMinusButtonClicked()
    {
        if (_curLv > 0)
        {
            _curLv--;
            OnElementsLevelChanged?.Invoke(_challengeData.idNumber, _curLv);
            RefreshUI() ;
        }
    }

    private void RefreshUI()
    {
        float effectValue = _challengeData.valuePerLevel * _curLv;
        int pointValue = _challengeData.pointPerLevel * _curLv;

        _effectValue.text = effectValue.ToString("+#;-#;+0") + "%";
        _points.text = $"+{pointValue}";
        _level.text = $"{_curLv}";

        _minusButton.interactable = _curLv > 0;
        _plusButton.interactable = _curLv < _challengeData.maxLevel;
    }

    public void ResetLevel()
    {
        _curLv = 0;
        RefreshUI();
    }
}
