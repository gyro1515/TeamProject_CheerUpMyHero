using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public struct EquipSlotViewModel
{
    public string Name;
    public string StatType;
    public string StatValue;
    public Color BorderColor;
    public Sprite Icon;
}

public class UIArtifactEquipSlot : MonoBehaviour
{
    #region UI요소 참조 변수
    [Header("유물 데이터 적용")]
    [SerializeField] private Image _artifactIcon;
    [SerializeField] private Outline _iconOutline;
    //[SerializeField] private TextMeshProUGUI _nameText;
    //[SerializeField] private TextMeshProUGUI _statTypeText;
    //[SerializeField] private TextMeshProUGUI _statValueText;

    private Button _button;
    private Outline _outline;
    private int _slotIndex;
    #endregion

    #region 이벤트 시스템
    public event Action<int> OnRequestOpenInventory;
    #endregion

    // 생성된 슬롯이 몇 번째 슬롯인지만 정보 넣어줌.
    public void Init(int slotIndex)
    {
        _slotIndex = slotIndex;
        
        _outline = GetComponent<Outline>();
        _button = GetComponent<Button>();
        _button.onClick.AddListener(OnButtonClicked);

        if (_artifactIcon != null ) _artifactIcon.preserveAspect = true;
    }

    // 슬롯에 유물 정보 넣어줌
    public void RefreshArtifactEquipSlotDisplay(EquipSlotViewModel vm)
    {
        if (string.IsNullOrEmpty(vm.Name))
        {
            _artifactIcon.sprite = null;
            _artifactIcon.color = Color.clear;
            
            //_nameText.text = "";
            //_statTypeText.text = "";
            //_statValueText.text = "";
            
            _outline.effectColor = Color.black;
            _iconOutline.effectColor = Color.clear;
        }
        else
        {
            _artifactIcon.sprite = vm.Icon;
            _artifactIcon.color = Color.white;

            //_nameText.text = vm.Name;
            //_statTypeText.text = vm.StatType;
            //_statValueText.text = vm.StatValue;

            _outline.effectColor = vm.BorderColor;
            _iconOutline.effectColor = vm.BorderColor;
        }
    }

    private void OnButtonClicked()
    {
        OnRequestOpenInventory?.Invoke(_slotIndex);
    }
}
