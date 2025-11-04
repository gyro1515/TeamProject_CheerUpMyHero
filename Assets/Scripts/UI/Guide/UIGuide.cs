using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class UIGuide : BaseUI
{
    [Header("버튼")]
    [SerializeField] private Button unitListButton;     
    [SerializeField] private Button artifactListButton; 
    [SerializeField] private Button BackButton;

    [Header("아이콘 그리드")]
    [SerializeField] private GameObject iconScrollView;        // 2. 스크롤 뷰
    [SerializeField] private Transform iconGridContentParent; // 2. Content (Grid Layout Group)
    [SerializeField] private GameObject iconPrefab;

    [SerializeField] private UIUnitexplanationPopup uiUnitexplanationPopup;
    [SerializeField] private UIAfExpanationPopup uiAfExpanationPopup;

    IEventPublisher<SpawnUnitSlotStartHoldEvent> spawnUnitSlotStartHoldEventPub;
    IEventPublisher<AfSlotStartHoldEvent> afSlotStartHoldEventPub;

    UIMenu UIMenu;
    private void Awake()
    {
        uiUnitexplanationPopup.Init();
        uiAfExpanationPopup.Init();

        spawnUnitSlotStartHoldEventPub = EventManager.GetPublisher<SpawnUnitSlotStartHoldEvent>();
        afSlotStartHoldEventPub = EventManager.GetPublisher<AfSlotStartHoldEvent>();

        unitListButton?.onClick.AddListener(PopulateUnitGrid);
        artifactListButton?.onClick.AddListener(PopulateArtifactGrid);
        BackButton?.onClick.AddListener(OnBackButtonClicked);
        //var baseUnitData = DataManager.PlayerUnitData.GetData(115004);
        //var atifactData = DataManager.ArtifactData.GetData(08010005);
        //Button1.onClick.AddListener(() => { spawnUnitSlotStartHoldEventPub?.Publish(new SpawnUnitSlotStartHoldEvent(baseUnitData)); });
        //Button2.onClick.AddListener(() => { afSlotStartHoldEventPub?.Publish(new AfSlotStartHoldEvent(atifactData)); });
    }
    void Start()
    {
        //PopulateUnitGrid();
        UIMenu = UIManager.Instance.GetUI<UIMenu>();

    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void ClearGrid()
    {
        foreach (Transform child in iconGridContentParent)
        {
            Destroy(child.gameObject);
        }
    }
    public void PopulateUnitGrid()
    {
        ClearGrid(); // 1. 그리드 비우기
        if (iconScrollView != null) iconScrollView.SetActive(true);

        // 2. DataManager에서 '모든' 유닛 리스트 가져오기 (PlayerUnitSO 참조)
        PlayerUnitSO unitSO = DataManager.PlayerUnitData.SO;

        // PlayerUnitSO에 있는 모든 유닛 리스트를 하나로 합칩니다.
        List<BaseUnitData> allUnits = new List<BaseUnitData>();
        //allUnits.AddRange(unitSO.hero_unit.Cast<BaseUnitData>()); // HeroData -> BaseUnitData
        //allUnits.AddRange(unitSO.hiller_unit);
        allUnits.AddRange(unitSO.allianceCommon);
        allUnits.AddRange(unitSO.allianceRare);
        allUnits.AddRange(unitSO.allianceEpic);

        // (선택) ID 순서대로 정렬
        allUnits = allUnits.OrderBy(u => u.idNumber).ToList();

        // 3. 리스트를 순회하며 아이콘 생성
        foreach (var unitData in allUnits)
        {
            if (unitData == null) continue;

            GameObject iconGO = Instantiate(iconPrefab, iconGridContentParent);
            // 프리팹의 Image 컴포넌트에 아이콘 설정
            iconGO.GetComponent<Image>().sprite = unitData.unitIconSprite;

            // 4. (핵심) 생성된 아이콘 버튼에 '3번' 기능(팝업 띄우기) 연결
            iconGO.GetComponent<Button>().onClick.AddListener(() =>
            {
                OnUnitIconClicked(unitData);
            });
        }
    }

    public void PopulateArtifactGrid()
    {
        ClearGrid(); // 1. 그리드 비우기
        if (iconScrollView != null) iconScrollView.SetActive(true);

        // 2. ArtifactManager에서 '모든' 유물 리스트 가져오기 (ArtifactSO 참조)
        ArtifactSO artifactSO = DataManager.ArtifactData.SO;

        List<ArtifactData> allArtifacts = new List<ArtifactData>();
        allArtifacts.AddRange(artifactSO.activeArtifacts.Cast<ArtifactData>()); // Active -> Base
        allArtifacts.AddRange(artifactSO.passiveArtifacts.Cast<ArtifactData>()); // Passive -> Base

        allArtifacts = allArtifacts.OrderBy(a => a.idNumber).ToList();

        // 3. 리스트를 순회하며 아이콘 생성
        foreach (var artifactData in allArtifacts)
        {
            if (artifactData == null) continue;

            GameObject iconGO = Instantiate(iconPrefab, iconGridContentParent);

            iconGO.GetComponent<Image>().sprite = Resources.Load<Sprite>(artifactData.iconSpritePath);

            // 4. (핵심) 생성된 아이콘 버튼에 '3번' 기능(팝업 띄우기) 연결
            iconGO.GetComponent<Button>().onClick.AddListener(() =>
            {
                OnArtifactIconClicked(artifactData);
            });
        }
    }
    private void OnUnitIconClicked(BaseUnitData unitData)
    {
        Debug.Log($"유닛 아이콘 클릭됨: {unitData.unitName}");
        spawnUnitSlotStartHoldEventPub?.Publish(new SpawnUnitSlotStartHoldEvent(unitData));
    }

    private void OnArtifactIconClicked(ArtifactData artifactData)
    {
        Debug.Log($"유물 아이콘 클릭됨: {artifactData.name}");
        afSlotStartHoldEventPub?.Publish(new AfSlotStartHoldEvent(artifactData));
    }
    private void OnBackButtonClicked()
    {
        FadeManager.Instance.SwitchGameObjects(this.gameObject, UIMenu.gameObject);
    }
}
