using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIGuide : BaseUI
{
    [SerializeField] private Button Button1;
    [SerializeField] private Button Button2;
    [SerializeField] private UIUnitexplanationPopup uiUnitexplanationPopup;
    [SerializeField] private UIAfExpanationPopup uiAfExpanationPopup;

    IEventPublisher<SpawnUnitSlotStartHoldEvent> spawnUnitSlotStartHoldEventPub;
    IEventPublisher<AfSlotStartHoldEvent> afSlotStartHoldEventPub;
    private void Awake()
    {
        uiUnitexplanationPopup.Init();
        uiAfExpanationPopup.Init();

        spawnUnitSlotStartHoldEventPub = EventManager.GetPublisher<SpawnUnitSlotStartHoldEvent>();
        afSlotStartHoldEventPub = EventManager.GetPublisher<AfSlotStartHoldEvent>();

        var baseUnitData = DataManager.PlayerUnitData.GetData(115004);
        var atifactData = DataManager.ArtifactData.GetData(08010005);
        Button1.onClick.AddListener(() => { spawnUnitSlotStartHoldEventPub?.Publish(new SpawnUnitSlotStartHoldEvent(baseUnitData)); });
        Button2.onClick.AddListener(() => { afSlotStartHoldEventPub?.Publish(new AfSlotStartHoldEvent(atifactData)); });
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
