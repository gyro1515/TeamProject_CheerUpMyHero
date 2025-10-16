using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.GraphicsBuffer;

public class UIRemoveAllAfPanel : MonoBehaviour
{
    [Header("일괄 장착 해제 패널 세팅")]
    [SerializeField] Button yesBtn;
    [SerializeField] Button noBtn;
    [SerializeField] CanvasGroup canvasGroup;
    public event Action OnRemoveAllAAf;

    private void Awake()
    {
        yesBtn.onClick.AddListener(RemoveAllEquipActiveArtifact);
        noBtn.onClick.AddListener(() => SetActive(false));
        canvasGroup.alpha = 0;
        SetActive(false);
    }
    void RemoveAllEquipActiveArtifact()
    {
        OnRemoveAllAAf?.Invoke();
        SetActive(false);
        yesBtn.enabled = false; noBtn.enabled = false;
    }
    public void SetActive(bool active)
    {
        if(active)
        {
            FadeManager.FadeInUI(canvasGroup, () => SetButtonActive(active));
        }
        else
        {
            SetButtonActive(active);
            FadeManager.FadeOutUI(canvasGroup);
        }
    }
    void SetButtonActive(bool active)
    {
        yesBtn.enabled = active; 
        noBtn.enabled = active;
    }
}
