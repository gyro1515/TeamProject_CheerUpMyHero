using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
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
    [SerializeField] private UIAfExpanationForGuide uiAfExpanationPopup;

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
        PopulateUnitGrid();
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
            iconGO.GetComponentInChildren<Image>().sprite = unitData.unitIconSprite;
            TextMeshProUGUI nameText = iconGO.GetComponentInChildren<TextMeshProUGUI>();
            if (nameText != null)
            {
                nameText.text = unitData.unitName;
                nameText.gameObject.SetActive(true); // 텍스트 켜기
            }
            Transform frameBorder = iconGO.transform.Find("FramBorder");
            if (frameBorder != null)
            {
                frameBorder.gameObject.SetActive(true); // 유닛이니까 액자 켜기
            }
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

        ArtifactSO artifactSO = DataManager.ArtifactData.SO;
        if (artifactSO == null)
        {
            Debug.LogError("DataManager에서 ArtifactSO를 불러오는 데 실패했습니다!");
            return;
        }

        // 2. 모든 유물 리스트를 하나로 합침
        List<ArtifactData> allArtifacts = new List<ArtifactData>();
        allArtifacts.AddRange(artifactSO.activeArtifacts.Cast<ArtifactData>());
        allArtifacts.AddRange(artifactSO.passiveArtifacts.Cast<ArtifactData>());

        // --- ✨ 3. 유물들을 "그룹 ID"로 그룹화합니다. ✨ ---
        // (가정: ID의 마지막 자리가 레벨(1~5)이므로, 10으로 나눈 몫이 그룹 ID입니다.)
        // 예: 080200041 ~ 080200045 -> 8020004 그룹
        var groupedArtifacts = allArtifacts
            .Where(a => a != null) // null이 아닌 것만
            .GroupBy(a =>
            {
                // 3-1. 이 유물이 '액티브' 유물인지 확인합니다.
                if (a is ActiveArtifactData)
                {
                    return a.idNumber;
                }
                else // 패시브 유물이라면
                {
                    return a.idNumber / 10;
                }
            }) 
            .OrderBy(g => g.Key); // ID 순서대로 정렬

        // --- 4.'모든 유물' 대신 '그룹'을 순회합니다. ✨ ---
        foreach (var artifactGroup in groupedArtifacts)
        {
            ArtifactData representativeArtifact = artifactGroup.First();
            if (representativeArtifact == null) continue;

            GameObject iconGO = Instantiate(iconPrefab, iconGridContentParent);

            // 6. 대표 유물의 아이콘 표시
            iconGO.GetComponentInChildren<Image>().sprite = Resources.Load<Sprite>(representativeArtifact.iconSpritePath);

            TextMeshProUGUI nameText = iconGO.GetComponentInChildren<TextMeshProUGUI>();
            if (nameText != null)
            {
                nameText.gameObject.SetActive(false); // 이름, 레벨 숨기기
            }
            Transform frameBorder = iconGO.transform.Find("FramBorder");
            if (frameBorder != null)
            {
                frameBorder.gameObject.SetActive(false); // 유물이니까 액자 끄기
            }

            // 8. (핵심) 클릭 시 '대표 유물'의 데이터를 팝업으로 보냅니다.
            iconGO.GetComponent<Button>().onClick.AddListener(() =>
            {
                OnArtifactIconClicked(representativeArtifact);
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
