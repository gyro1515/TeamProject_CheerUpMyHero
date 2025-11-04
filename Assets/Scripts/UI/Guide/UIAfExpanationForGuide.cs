using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class UIAfExpanationForGuide : MonoBehaviour
{
    [SerializeField] private Image descriptionIcon;
    [SerializeField] private TextMeshProUGUI descriptionName;
    [SerializeField] private TextMeshProUGUI description;

    bool _isFade = false;
    CanvasGroup _canvasGroup;
    IEventSubscriber<AfSlotStartHoldEvent> afSlotStartHoldEventSub;
    IEventSubscriber<AfSlotReleaseHoldEvent> afSlotReleaseHoldEventSub;
    public void Init()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        _canvasGroup.alpha = 0f;
        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = false;
        afSlotStartHoldEventSub = EventManager.GetSubscriber<AfSlotStartHoldEvent>();
        afSlotStartHoldEventSub?.Subscribe(OpenDescriptionPanel);
        afSlotReleaseHoldEventSub = EventManager.GetSubscriber<AfSlotReleaseHoldEvent>();
        afSlotReleaseHoldEventSub?.Subscribe(CloseDescriptionPanel);
    }
    private void OnDestroy()
    {
        afSlotStartHoldEventSub?.Unsubscribe(OpenDescriptionPanel);
        afSlotReleaseHoldEventSub?.Unsubscribe(CloseDescriptionPanel);
    }
    void OpenDescriptionPanel(AfSlotStartHoldEvent startHoldEvent)
    {
        OpenUI(0.05f);
        descriptionName.text = startHoldEvent.artifactData.name;
        descriptionIcon.sprite = Resources.Load<Sprite>(startHoldEvent.artifactData.iconSpritePath);
        description.text = startHoldEvent.artifactData.descriptionForGuide;
    }
    void CloseDescriptionPanel(AfSlotReleaseHoldEvent releaseHoldEvent)
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