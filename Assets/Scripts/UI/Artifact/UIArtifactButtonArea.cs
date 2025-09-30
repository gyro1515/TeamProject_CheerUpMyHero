using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIArtifactButtonArea : MonoBehaviour
{
    [Header("버튼")]
    [SerializeField] private Button _passiveEquipButton;
    [SerializeField] private Button _activeEquipButton;
    [SerializeField] private Button _allUnEquipButton;

    private void Awake()
    {
        
    }

    private void Start()
    {
        _passiveEquipButton.onClick.AddListener(() => ArtifactManager.Instance.AutoEquipArtifacts(ArtifactType.Passive));
        _activeEquipButton.onClick.AddListener(() => ArtifactManager.Instance.AutoEquipArtifacts(ArtifactType.Active));
    }

    private void OnPassiveShowButtonClicked()
    {

    }   
    
    private void OnActivaShowButtonClicked()
    {

    }

    private void OnAutoEquipButtonClicked()
    {

    }
}
