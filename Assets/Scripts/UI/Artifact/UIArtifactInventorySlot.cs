using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System;

public struct InventorySlotViewModel
{
    public ArtifactData Artifact;
    public string Name;
    public string StatType;
    public string StatValue;
    public Color BorderColor;
    public Sprite Icon;
    public bool IsEquippedInCurrentSlot;
}

public class UIArtifactInventorySlot : MonoBehaviour
{
    #region UI요소 참조 변수
    [Header("유물 데이터 적용")]
    [SerializeField] private Image _artifactIcon;
    [SerializeField] private Outline _iconOutline;
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private TextMeshProUGUI _statTypeText;
    [SerializeField] private TextMeshProUGUI _statValueText;
    [SerializeField] private GameObject _equippedImage;

    private ArtifactData _data;
    private Button _button;
    private Outline _outline;
    #endregion

    #region 이벤트 시스템
    public event Action<ArtifactData> OnArtifactInventorySlotClicked;
    #endregion

    public void Init(InventorySlotViewModel vm)
    {
        _outline = GetComponent<Outline>();
        _button = GetComponent<Button>();
        _button.onClick.AddListener(OnButtonClicked);

        _data = vm.Artifact;

        _nameText.text = vm.Name;
        _statTypeText.text = vm.StatType;
        _statValueText.text = vm.StatValue;
        _outline.effectColor = vm.BorderColor;
        _iconOutline.effectColor = vm.BorderColor;
        _artifactIcon.sprite = vm.Icon;
        _artifactIcon.preserveAspect = true;
        _equippedImage.SetActive(vm.IsEquippedInCurrentSlot);

        gameObject.SetActive(true);
    }

    private void OnButtonClicked()
    {
        // 예시
        //AudioManager.PlayOneShot(DataManager.AudioData.artifactEquipSE, 1.0f);
        OnArtifactInventorySlotClicked?.Invoke(_data);
    }
}
