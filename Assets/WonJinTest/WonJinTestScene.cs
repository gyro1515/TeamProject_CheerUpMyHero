using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WonJinTestScene : SceneBase
{
    public override void OnSceneEnter()
    {
        // 유저가 선택한 맵 소환
        // ex) 1-6을 선택했다면 이에 해당하는 메인스테이지 인덱스와, 서브 스테이지 인덱스가 있을거고
        // $"Prefab/Map/Map{플레이어 데이터의 SelectedMainSlotIdx}_{플레이어 데이터의SelectedSubSlotIdx}"
        // 이런 형식으로 소환하면 될 것
        // 현재 테스트로 그냥 소환
        var map = Resources.Load<GameObject>("Prefab/Map/TestMap");
        //Instantiate(map);
    }

    public override void OnSceneExit()
    {
    }

    public override void SceneLoading()
    {
    }
}
