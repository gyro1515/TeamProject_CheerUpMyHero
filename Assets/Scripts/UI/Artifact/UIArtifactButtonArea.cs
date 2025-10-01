using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIArtifactButtonArea : MonoBehaviour
{
    [Header("버튼")]
    [SerializeField] private Button _passiveEquipButton;
    [SerializeField] private Button _activeEquipButton;
    [SerializeField] private Button _ConfirmEquipButton;

    [Header("선택 아웃라인")]
    [SerializeField] private Outline _passiveOutline;
    [SerializeField] private Outline _activeOutline;
    private ArtifactType _selectedType;

    private void Awake()
    {
        _passiveEquipButton.onClick.AddListener(OnPassiveEquipButtonClicked);
        _activeEquipButton.onClick.AddListener(OnActiveEquipButtonClicked);

        if (_passiveOutline != null) _passiveOutline.enabled = false;
        if (_activeOutline != null) _activeOutline.enabled = false;
    }

    private void Start()
    {
        _ConfirmEquipButton.onClick.AddListener(() => ArtifactManager.Instance.AutoEquipArtifacts(_selectedType));
    }

    private void OnPassiveEquipButtonClicked()
    {
        _selectedType = ArtifactType.Passive;
        UpdateSelectionUI();

    }

    private void OnActiveEquipButtonClicked()
    {
        _selectedType = ArtifactType.Active;
        UpdateSelectionUI();
    }

    private void UpdateSelectionUI()
    {
        if (_passiveOutline != null)
            _passiveOutline.enabled = (_selectedType == ArtifactType.Passive);

        if (_activeOutline != null)
            _activeOutline.enabled = (_selectedType == ArtifactType.Active);
    }
}
