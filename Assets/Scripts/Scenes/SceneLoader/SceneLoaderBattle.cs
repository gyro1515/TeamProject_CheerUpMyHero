using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneLoaderBattle : MonoBehaviour
{
    [SerializeField] GameObject map;
    private void Awake()
    {
        UIManager.Instance.GetUI<UITest>();
        //GameManager에게 전투 시작 준비를 명령
        GameManager.Instance.StartBattle(); //배틀씬으로 갔을 때부터 식량 획득 증가 함수
        // 유저가 선택한 맵 소환
        // ex) 1-6을 선택했다면 이에 해당하는 메인스테이지 인덱스와, 서브 스테이지 인덱스가 있을거고
        // $"Prefab/Map/Map{플레이어 데이터의 SelectedMainSlotIdx}_{플레이어 데이터의SelectedSubSlotIdx}"
        // 이런 형식으로 소환하면 될 것
        /*(int mainStageIdx, int subStageIdx) = PlayerDataManager.Instance.SelectedStageIdx;
        var map = Resources.Load<GameObject>($"Prefab/Map/Map{mainStageIdx}_{subStageIdx}");*/

        // 현재는 연결된 맵 소환
        Instantiate(map);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SceneLoader.Instance.StartLoadScene(SceneState.MainScene);
        }
    }
}

