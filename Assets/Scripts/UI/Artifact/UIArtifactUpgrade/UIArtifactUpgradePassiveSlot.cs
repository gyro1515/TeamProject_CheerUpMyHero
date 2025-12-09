using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIArtifactUpgradePassiveSlot : MonoBehaviour
{
    [Header("유물 정보 UI 참조")]
    [SerializeField] private Image _icon;
    [SerializeField] private Outline _iconOutline;
    [SerializeField] private TextMeshProUGUI _countText;

    private Button _button;
    private Outline _slotOutline;
    private int _idNumber;

    public event Action<int> OnPassiveSlotClicked;

    private void Awake()
    {
        _button = GetComponent<Button>();
        _slotOutline = GetComponent<Outline>();
        _button.onClick.AddListener(OnSlotButtonClicked);
    }

    public void Init(PassiveSlotViewModel vm)
    {
        if (vm.Artifact != null)
        {
            _idNumber = vm.Artifact.idNumber;

            _icon.sprite = vm.Icon;
            _icon.color = Color.white;
            _iconOutline.effectColor = vm.BorderColor;
            _slotOutline.effectColor = vm.BorderColor;
            _countText.text = vm.OwnedCount.ToString();
        }
        else
        {
            _icon.sprite = null;
            _icon.color = Color.clear;
            _iconOutline.effectColor = Color.gray;
            _slotOutline.effectColor= Color.gray;
            _countText.text = "";
        }

        _button.interactable = vm.IsSelectable;
    }

    private void OnSlotButtonClicked()
    {
        OnPassiveSlotClicked?.Invoke(_idNumber);
    }
}
