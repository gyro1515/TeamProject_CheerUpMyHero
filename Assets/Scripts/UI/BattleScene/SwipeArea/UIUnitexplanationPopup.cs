using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]

public class UIUnitexplanationPopup : MonoBehaviour
{
    [SerializeField] UIUnitCardInScroll card;

    bool _isFade = false;
    CanvasGroup _canvasGroup;
    IEventSubscriber<SpawnUnitSlotStartHoldEvent> spawnUnitSlotStartHoldEventSub;
    IEventSubscriber<SpawnUnitSlotReleaseHoldEvent> spawnUnitSlotReleaseHoldEventSub;
    public void Init()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        _canvasGroup.alpha = 0f;
        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = false;
        spawnUnitSlotStartHoldEventSub = EventManager.GetSubscriber<SpawnUnitSlotStartHoldEvent>();
        spawnUnitSlotStartHoldEventSub?.Subscribe(OpenDescriptionPanel);
        spawnUnitSlotReleaseHoldEventSub = EventManager.GetSubscriber<SpawnUnitSlotReleaseHoldEvent>();
        spawnUnitSlotReleaseHoldEventSub?.Subscribe(CloseDescriptionPanel);
    }
    private void OnDestroy()
    {
        spawnUnitSlotStartHoldEventSub?.Unsubscribe(OpenDescriptionPanel);
        spawnUnitSlotReleaseHoldEventSub?.Unsubscribe(CloseDescriptionPanel);
    }
    void OpenDescriptionPanel(SpawnUnitSlotStartHoldEvent startHoldEvent)
    {
        OpenUI(0.05f);
        card.UpdateCardDataByData(startHoldEvent.unitData);
    }
    void CloseDescriptionPanel(SpawnUnitSlotReleaseHoldEvent releaseHoldEvent)
    {
        CloseUI(0.05f);
    }
    void OpenUI(float fadeTime)
    {
        if (_isFade) return;
        gameObject.SetActive(true);
        _isFade = true;
        FadeManager.FadeInUI(_canvasGroup, SetFadeFalse, true, fadeTime);
    }
    void CloseUI(float fadeTime)
    {
        //if (_isFade) return;
        _isFade = true;
        FadeManager.FadeOutUI(_canvasGroup, () => { gameObject.SetActive(false); SetFadeFalse(); }, true, fadeTime);
    }
    protected void SetFadeFalse()
    {
        _isFade = false;
    }
}
