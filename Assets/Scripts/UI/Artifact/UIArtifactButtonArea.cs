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

    private ArtifactType _selectedType;

    private void Awake()
    {
        _passiveEquipButton.onClick.AddListener(OnPassiveEquipButtonClicked);
        _activeEquipButton.onClick.AddListener(OnActiveEquipButtonClicked);
    }

    private void Start()
    {
        _ConfirmEquipButton.onClick.AddListener(() => ArtifactManager.Instance.AutoEquipArtifacts(_selectedType));
    }

    private void OnPassiveEquipButtonClicked()
    {
        _selectedType = ArtifactType.Passive;
    }   
    
    private void OnActiveEquipButtonClicked()
    {
        _selectedType = ArtifactType.Active;
    }
}
