using DG.Tweening;
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
    [SerializeField] UIEquippedPanel equippedPanel;
    [SerializeField] CanvasGroup canvasGroup;

    private void Awake()
    {
        yesBtn.onClick.AddListener(RemoveAllEquipActiveArtifact);
        noBtn.onClick.AddListener(() => { SetActive(false); });
        canvasGroup.alpha = 0;
        gameObject.SetActive(false);
    }
    void RemoveAllEquipActiveArtifact()
    {
        equippedPanel.RemoveAllEquipActiveArtifact();
        //FadeEffectManager.Instance.FadeOutUI(canvasGroup);
        SetActive(false);
    }
    public void SetActive(bool active)
    {
        if(active)
        {
            gameObject.SetActive(true);
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
            canvasGroup.DOFade(1f, 0.3f);
        }
        else
        {
            canvasGroup.DOFade(0f, 0.3f).onComplete += () => gameObject.SetActive(false);
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
    }
}
