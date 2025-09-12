using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIStageSelect : BaseUI
{
    [Header("스테이지 선택UI 설정")]
    [SerializeField] GameObject mainStageSelectGO;
    [SerializeField] GameObject subStageSelectGO;
    [Header("스테이지 선택UI 테스트 용")]
    //리스트 개수 =  메인 스테이지 개수, 리스트 내용 = 메인 스테이지 이름
    [SerializeField] List<string> mainStageList = new List<string>();
    //리스트 개수 =  메인 스테이지 개수, 리스트 내용 = 스테이지 단계 수
    [SerializeField] List<int> subStageList = new List<int>();

}
