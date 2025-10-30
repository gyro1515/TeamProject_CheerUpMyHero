using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static UIPause;

public class SettingDataManager : SingletonMono<SettingDataManager>
{
    public List<MainStageData> MainStageData { get; private set; } = new();
   
    public static event Action OnControlLayoutChanged;

    SpeedState _savedSpeed = SpeedState.X1;
    public static SpeedState SavedSpeed { get => Instance._savedSpeed; set => Instance._savedSpeed = value; }

    protected override void Awake()
    {
        base.Awake();

        LoadStageDataFromSO();
        LoadLayoutSetting();
    }

    void LoadStageDataFromSO()
    {
        //메인 스테이지 데이터 SO로부터 불러오기

        int mainStageSize = DataManager.Instance.MainStageData.GetDataSize();

        for (int i = 1; i <= mainStageSize; i++)
        {
            MainStageData.Add(DataManager.Instance.MainStageData.GetData(i));
        }


        //서브 스테이지 데이터 SO로부터 불러와서 캐싱된 메인 스테이지 데이터에 합치기
        StringBuilder sb = new();

        for (int i = 0; i < mainStageSize; i++)
        {
            int subStageCount = MainStageData[i].subStageCount;

            for (int j = 0; j < subStageCount; j++)
            {
                sb.Append(i + 1).Append(0).Append(0).Append(j + 1);
                string indexSTr = sb.ToString();
                bool successCast = int.TryParse(indexSTr, out int index);
                if (successCast)
                    MainStageData[i].subStages.Add(DataManager.Instance.SubStageData.GetData(index));
                else
                    Debug.Log($"[SettingDataManager] 문자열로 index를 만들 수 없습니다.");
                sb.Clear();
            }
        }
    }
    #region 조작패널 설정 저장
    public int ControlPanelLayoutType { get; private set; } // 배틀씬 하단 레이아웃 설정 값 | 0 : 기본값 | 1 : 바꾼 값
    public const string ControlPanelLayoutTypeKey = "ControlPanelLayoutType";

    public void SetLayoutSetting(int type)
    {
        if (ControlPanelLayoutType == type) return;

        ControlPanelLayoutType = type;
        PlayerPrefs.SetInt(ControlPanelLayoutTypeKey, ControlPanelLayoutType);
        PlayerPrefs.Save();

        OnControlLayoutChanged?.Invoke();
    }

    public void LoadLayoutSetting()
    {
        ControlPanelLayoutType =  PlayerPrefs.GetInt(ControlPanelLayoutTypeKey, 0);
    }
    #endregion

    public void UnlockStage(int mainIndex, int subIndex)
    {
        MainStageData[mainIndex].subStages[subIndex].isUnlocked = true;
    }


    public List<List<bool>> SaveClearData()
    {
        List<List<bool>> boolListList = new ();
        
        for (int i = 0; i < MainStageData.Count; i++)
        {
            List<bool> boolList = new ();
            
            for(int j = 0; j < MainStageData[i].subStages.Count; j++)
            {
                boolList.Add(MainStageData[i].subStages[j].isUnlocked);
            }

            boolListList.Add(boolList);
        }

        return boolListList;
    }

    public void LoadClearData(List<List<bool>> boolListList)
    {
        for (int i = 0; i < boolListList.Count; i++)
        {
            for (int j = 0; j < boolListList[i].Count; j++)
            {
                bool result = boolListList[i][j];
                if (result)
                {
                    if(j == 0)
                    {
                        MainStageData[i].isUnlocked = true;
                    }

                    
                    MainStageData[i].subStages[j].isUnlocked = result;
                    Debug.Log($"{i + 1}-{j + 1} 클리어 결과 로드");
                }
            }
        }
    }
}