using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// 나중에 플레이어 데이터에 들어갈 것들
//[System.Serializable]
//public class SubStageData
//{
//    public string displayName;      // "1-1" 같은 표시용 이름
//    public bool isUnlocked = false; // 해금 여부
//}

//[System.Serializable]
//public class MainStageData
//{
//    public string displayName;          // "1. 겨울왕국"
//    public bool isUnlocked = false;     // 메인 스테이지 해금 여부
//    public List<SubStageData> subStages = new List<SubStageData>(); // 서브 스테이지 리스트
//}
public class UIStageSelect : BaseUI
{
    [Header("스테이지 선택UI 설정")]
    [SerializeField] Transform stageSlotsParent; // 슬롯들이 생성될 부모
    [SerializeField] GameObject selectSlotPrefab;
    [SerializeField] Button returnToSelDeckBtn;

    private DeckPresetController _deckPresetController;

    private void Awake()
    {
        _deckPresetController = UIManager.Instance.GetUI<DeckPresetController>();

        // 돌아가기 버튼 설정
        returnToSelDeckBtn.onClick.AddListener(MoveToSelDeck);

        // 모든 스테이지 슬롯을 생성하고 초기화
        CreateAllStageSlots();
    }

    /// 모든 메인/서브 스테이지를 하나의 목록으로 생성하는 함수
    private void CreateAllStageSlots()
    {
        // 기존에 생성된 슬롯이 있다면 모두 삭제
        foreach (Transform child in stageSlotsParent)
        {
            Destroy(child.gameObject);
        }

        // SettingDataManager에서 전체 스테이지 데이터를 가져옵니다.
        List<MainStageData> allStageData = SettingDataManager.Instance.MainStageData;

        // 모든 메인 스테이지를 순회
        for (int mainIndex = 0; mainIndex < allStageData.Count; mainIndex++)
        {
            for (int subIndex = 0; subIndex < allStageData[mainIndex].subStages.Count; subIndex++)
            {
                UISelecStageSlot slot = Instantiate(selectSlotPrefab, stageSlotsParent).GetComponent<UISelecStageSlot>();
                SubStageData subData = allStageData[mainIndex].subStages[subIndex];

                string stageName = subData.displayName;
                bool isUnlocked = allStageData[mainIndex].isUnlocked && subData.isUnlocked;

                slot.Initialize(stageName, isUnlocked);

                int capturedMainIndex = mainIndex;
                int capturedSubIndex = subIndex;

                slot.SelectButton.onClick.AddListener(() => MoveToBattle(capturedMainIndex, capturedSubIndex));
            }
        }
    }

    void MoveToBattle(int mainIdx, int subIdx)
    {
        Debug.Log($"{mainIdx + 1}-{subIdx + 1} 전투 씬으로 이동");

        // PlayerDataManager에 선택된 스테이지 정보를 저장
        PlayerDataManager.Instance.SelectedStageIdx = (mainIdx, subIdx);

        // 전투 씬을 로드
        SceneLoader.Instance.StartLoadScene(SceneState.BattleScene);
    }

    void MoveToSelDeck()
    {
        Debug.Log("덱 선택으로 이동");
        FadeManager.Instance.SwitchGameObjects(gameObject, _deckPresetController.gameObject);
    }
}