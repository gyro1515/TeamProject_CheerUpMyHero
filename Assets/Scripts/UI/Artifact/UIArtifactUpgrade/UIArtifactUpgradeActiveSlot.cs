using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIArtifactUpgradeActiveSlot : MonoBehaviour
{
    [Header("슬롯 UI")]
    [SerializeField] private Image _icon;
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private TextMeshProUGUI _levelText;

    private Button _button;
    private ActiveArtifactData _artifact;

    public event Action<ActiveArtifactData> OnActiveSlotClicked;

    private void Awake()
    {
        _button = GetComponent<Button>();
        _button.onClick.AddListener(OnButtonCLicked);
    }

    public void Init(ActiveSlotViewModel vm)
    {
        _artifact = vm.Artifact;

        if (vm.Artifact != null)
        {
            _icon.sprite = vm.Icon;
            _icon.color = Color.white;
            _nameText.text = vm.NameText;
            _levelText.text = vm.LevelText;
            _button.interactable = true;
        }
        else
        {
            _icon.sprite = null;
            _icon.color = Color.clear;
            _nameText.text = "";
            _levelText.text = "";
            _button.interactable = false;
        }
    }

    private void OnButtonCLicked()
    {
        if (_artifact != null)
        {
            OnActiveSlotClicked?.Invoke(_artifact);
        }
    }
}
