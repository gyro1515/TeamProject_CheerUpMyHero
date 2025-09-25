using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UIUnEquipBtn : MonoBehaviour
{
    [Header("장착 해제 버튼 세팅")]
    [SerializeField] Button unEquipBtn;
    [SerializeField] CanvasGroup canvasGp;
    private void OnEnable()
    {
        SetBtnActive(false);
    }
    public void Init(UnityAction action)
    {
        unEquipBtn.onClick.AddListener(action);
    }
    public void SetBtnActive(ActiveAfData data)
    {
        if (data == null || !data.isEquipped)
        {
            SetBtnActive(false);
        }
        else SetBtnActive(true);
    }
    void SetBtnActive(bool active)
    {
        canvasGp.interactable = active;
        canvasGp.blocksRaycasts = active;
        canvasGp.alpha = active ? 1f : 0.5f;
    }
}
