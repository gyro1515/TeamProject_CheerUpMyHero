using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneLoaderBattle : MonoBehaviour
{
    [SerializeField] GameObject map;
    private void Awake()
    {
        UIManager.Instance.GetUI<UITest>();
        UIManager.Instance.GetUI<UIPause>();
        UIManager.Instance.GetUI<UIHpBarContainer>();
        UnitManager.Instance.gameObject.SetActive(true);// 유닛 매니저는 싱글톤이지만, 씬 전환 시 파괴됨

        /*//GameManager에게 전투 시작 준비를 명령
        GameManager.Instance.StartBattle(); //배틀씬으로 갔을 때부터 식량 획득 증가 함수*/
        // 유저가 선택한 맵 소환
        // ex) 1-6을 선택했다면 이에 해당하는 메인스테이지 인덱스와, 서브 스테이지 인덱스가 있을거고
        // $"Prefab/Map/Map{플레이어 데이터의 SelectedMainSlotIdx}_{플레이어 데이터의SelectedSubSlotIdx}"
        // 이런 형식으로 소환하면 될 것
        (int mainStageIdx, int subStageIdx) = PlayerDataManager.Instance.SelectedStageIdx;
        if (mainStageIdx == -1 || subStageIdx == -1) // 선택된 스테이지가 없다면
        {
            Debug.Log("선택된 스테이지가 없습니다! 1-1 스테이지를 세팅합니다.");
            // 기본값 1-1로 세팅
            PlayerDataManager.Instance.SelectedStageIdx = (0, 0);
            mainStageIdx = 0;
            subStageIdx = 0;
        }
        var map = Resources.Load<GameObject>($"Map/Map{mainStageIdx + 1}_{subStageIdx + 1}");
        //Debug.Log($"Prefab/Map/Map{mainStageIdx + 1}_{subStageIdx + 1}");
        // 현재는 연결된 맵 소환
        Instantiate(map);
        AudioManager.PlayOneShot(DataManager.AudioData.BattleStartSE);

        // 초기화 순서상 UIPause보다 뒤에 있어야 함 따라서 UIPause Start()에서 호출로 변경
        /*if (!GameManager.IsTutorialCompleted)
        {
            UIManager.Instance.GetUI<UITutorialBattle>();
        }*/
    }

    /*private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SceneLoader.Instance.StartLoadScene(SceneState.MainScene);
        }
    }*/
}

