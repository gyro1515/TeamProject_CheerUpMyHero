using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UISynergyIconPressHandler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler, IEndDragHandler
{
    Image icon;
    BaseUnitData data;
    UnitSynergyType type;
    IEventPublisher<UISynergyIconPressedEvent> uiSynergyIconPressedEventPub;
    public void Init()
    {
        icon = GetComponent<Image>();
        gameObject.AddComponent<Button>().transition = Selectable.Transition.None;
        uiSynergyIconPressedEventPub = EventManager.GetPublisher<UISynergyIconPressedEvent>();
    }
    public void SetData(Sprite sprite, UnitSynergyType type, BaseUnitData data)
    {
        icon.sprite = sprite;
        this.data = data;
        this.type = type;
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log($"아이콘 {icon.sprite} 눌림, 유닛 데이터: {data}");
        uiSynergyIconPressedEventPub.Publish(new UISynergyIconPressedEvent(data, true, type));
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        Debug.Log($"아이콘 {icon.sprite} 땜, 유닛 데이터: {data}");
        uiSynergyIconPressedEventPub.Publish(new UISynergyIconPressedEvent(data, false, type));
    }
    public void OnDrag(PointerEventData eventData)
    {
        //uiSynergyIconPressedEventPub.Publish(new UISynergyIconPressedEvent(data, false, type));

    }
    public void OnEndDrag(PointerEventData eventData)
    {
        uiSynergyIconPressedEventPub.Publish(new UISynergyIconPressedEvent(data, false, type));

    }


}
#region 카드 안의 시너지 아이콘 눌림 이벤트
public struct UISynergyIconPressedEvent
{
    public BaseUnitData unitData;
    public UnitSynergyType synergyType;
    public bool isPressed;
    public UISynergyIconPressedEvent(BaseUnitData unitData, bool isPressed, UnitSynergyType synergyType)
    {
        this.unitData = unitData;
        this.isPressed = isPressed;
        this.synergyType = synergyType;
    }
}
#endregion
