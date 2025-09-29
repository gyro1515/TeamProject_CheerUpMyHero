//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;



//// 나중에 플레이어 데이터에 들어갈 것들
////[System.Serializable]
////public class SubStageData
////{
////    public string displayName;      // "1-1" 같은 표시용 이름
////    public bool isUnlocked = false; // 해금 여부
////}

////[System.Serializable]
////public class MainStageData
////{
////    public string displayName;          // "1. 겨울왕국"
////    public bool isUnlocked = false;     // 메인 스테이지 해금 여부
////    public List<SubStageData> subStages = new List<SubStageData>(); // 서브 스테이지 리스트
////}
//public class UIStageSelect : BaseUI
//{
//    [Header("스테이지 선택UI 설정")]
//    [SerializeField] GameObject mainStageSelectGO; // 온/오프 용
//    [SerializeField] GameObject subStageSelectGO; // 온/오프 용
//    [SerializeField] Transform mainStageSlotsTransform;
//    [SerializeField] Transform subStageSlotsTransform;
//    [SerializeField] GameObject selectSlotPrefab;
//    [SerializeField] Button returnToSelDeckBtn;
//    [SerializeField] Button returnToSelMainBtn;
//    [Header("스테이지 선택UI 테스트 용")]
//    //리스트 개수 = 메인 스테이지 개수
//    [SerializeField] List<MainStageData> stageList = new List<MainStageData>();
//    // 메인 스테이지 슬롯 리스트
//    List<UISelecStageSlot> mainSlotList = new List<UISelecStageSlot>();
//    // 메인 스테이지에 따라 활성화할 서브 스테이즈 슬롯들
//    List<UISelecStageSlot> subSlotList = new List<UISelecStageSlot>();
//    // 최대 서브 스테이지 개수
//    int maxSubSlot = 9;
//    // 서브 스테이지 선택에서 되돌아가면 스크롤 위치 초기화하는 용
//    RectTransform subStageSelRectTransform;

//    public int SelectedMainSlotIdx { get; set; } = -1;
//    public int SelectedSubSlotIdx { get; set; } = -1;

//    //UISelectCard uiSelectCard;
//    private DeckPresetController _deckPresetController;

//    private void Awake()
//    {
//        subStageSelRectTransform = subStageSlotsTransform.GetComponent<RectTransform>();
//        _deckPresetController = UIManager.Instance.GetUI<DeckPresetController>();

//        // 테스트 용으로 스테이지 데이터 세팅하기
//        //TestForStageData();
//        stageList = SettingDataManager.Instance.MainStageData;


//        // 슬롯 생성
//        for (int i = 0; i < stageList.Count; i++)
//        {
//            UISelecStageSlot slot = Instantiate(selectSlotPrefab, mainStageSlotsTransform).GetComponent<UISelecStageSlot>();
//            slot.InitSlot(stageList[i].displayName, i, stageList[i].isUnlocked, this, ESelecStageSlotType.Main);
//            slot.SelectButton.onClick.AddListener(MoveToSelSubStage);
//            mainSlotList.Add(slot);
//        }
//        // 최대 서브 스테이지 개수에 따라 먼저 슬롯 생성하기, 현재 9
//        for (int i = 0; i < maxSubSlot; i++)
//        {
//            UISelecStageSlot slot = Instantiate(selectSlotPrefab, subStageSlotsTransform).GetComponent<UISelecStageSlot>();
//            slot.InitSlot("", i, false, this, ESelecStageSlotType.Sub);
//            slot.SelectButton.onClick.AddListener(MoveToBattle);
//            subSlotList.Add(slot);
//        }
//        // 돌아가기 버튼 세팅
//        returnToSelDeckBtn.onClick.AddListener(MoveToSelDeck);
//        returnToSelMainBtn.onClick.AddListener(MoveToSelMainStage);
//        // uiSelectCard = UIManager.Instance.GetUI<UISelectCard>();
//    }
//    private void OnEnable()
//    {
//        SetMainSlot();
//    }
//    void MoveToSelSubStage()
//    {
//        SetSubSlot(SelectedMainSlotIdx);
//        FadeManager.Instance.SwitchGameObjects(mainStageSelectGO, subStageSelectGO);
//    }
//    void MoveToSelMainStage()
//    {
//        FadeManager.Instance.SwitchGameObjects(subStageSelectGO, mainStageSelectGO);
//    }
//    void MoveToBattle()
//    {
//        Debug.Log($"{SelectedMainSlotIdx + 1}-{SelectedSubSlotIdx + 1} 전투 씬으로 이동");
//        PlayerDataManager.Instance.SelectedStageIdx = (SelectedMainSlotIdx, SelectedSubSlotIdx);
//        SceneLoader.Instance.StartLoadScene(SceneState.BattleScene);
//    }
//    void MoveToSelDeck()
//    {
//        Debug.Log("덱 선택으로 이동");
//        FadeManager.Instance.SwitchGameObjects(gameObject, _deckPresetController.gameObject);
//    }
//    void SetMainSlot()
//    {
//        for (int i = 0; i < mainSlotList.Count; i++)
//        {
//            mainSlotList[i].SetSlotUnLocked(stageList[i].isUnlocked);
//        }
//    }
//    void SetSubSlot(int mainStageIdx)
//    {
//        Vector3 resetPos = subStageSlotsTransform.position;
//        resetPos.y = 0;
//        subStageSlotsTransform.position = resetPos;
//        for (int i = 0; i < maxSubSlot; i++)
//        {
//            if (i < stageList[mainStageIdx].subStages.Count)
//            {
//                subSlotList[i].gameObject.SetActive(true);
//                SubStageData subData = stageList[mainStageIdx].subStages[i];
//                // 현재는 1-1, 9-4 등으로 서브 스테이지가 정의되어 있어서 subData.displayName이 필요 없지만
//                // 나중은 모르니까 일단 subData.displayName 남기기
//                //--> 부활
//                subSlotList[i].SetSlot(subData.displayName, i, subData.isUnlocked);

//                //subSlotList[i].SetSlot($"{mainStageIdx + 1}-{i + 1}", i, subData.isUnlocked);
//            }
//            else
//            {
//                subSlotList[i].gameObject.SetActive(false);
//            }
//        }
//    }


//    void TestForStageData()
//    {
//        // 스테이지 1은 9개,2는 8, 3-7, 4-6, 5-5, 6-6, 7-7, 8-8, 9-9
//        for (int i = 0; i < stageList.Count; i++)
//        {
//            stageList[i].subStages.Clear();
//            if (i < 5)
//            {
//                for (int j = 1; j <= stageList.Count - i; j++)
//                {
//                    SubStageData tmpSub = new SubStageData();
//                    tmpSub.displayName = $"{i + 1}-{j}";
//                    stageList[i].subStages.Add(tmpSub);
//                }
//            }
//            else
//            {
//                for (int j = 1; j <= i + 1; j++)
//                {
//                    SubStageData tmpSub = new SubStageData();
//                    tmpSub.displayName = $"{i + 1}-{j}";
//                    stageList[i].subStages.Add(tmpSub);
//                }
//            }

//        }
//        // 1-1 ~ 1-3 해금
//        stageList[0].isUnlocked = true;
//        stageList[0].subStages[0].isUnlocked = true;
//        stageList[0].subStages[1].isUnlocked = true;
//        stageList[0].subStages[2].isUnlocked = true;
//        // 9-7 ~ 9-9 해금
//        stageList[8].isUnlocked = true;
//        stageList[8].subStages[8].isUnlocked = true;
//        stageList[8].subStages[7].isUnlocked = true;
//        stageList[8].subStages[6].isUnlocked = true;
//        // 4-3 ~ 4-5 해금
//        stageList[3].isUnlocked = true;
//        stageList[3].subStages[2].isUnlocked = true;
//        stageList[3].subStages[3].isUnlocked = true;
//        stageList[3].subStages[4].isUnlocked = true;
//    }
//}


//// Update is called once per frame
//void Update()
//    {
        
//    }
//}
