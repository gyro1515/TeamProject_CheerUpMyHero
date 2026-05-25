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

    #region 웨이브 워닝 중에 속도 변경 여부
    bool isSpeedChangedInWaring = false;
    public static bool IsSpeedChangedInWaring { 
        get 
        {
            if (!Instance) return false;
            return Instance.isSpeedChangedInWaring;
        }
        set
        {
            if (!Instance) return;
            Instance.isSpeedChangedInWaring = value;
        } }
    #endregion

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
        if (mainIndex < 0 || mainIndex >= MainStageData.Count)
        {
            Debug.LogWarning($"[SettingDataManager] UnlockStage: mainIndex({mainIndex})가 범위를 벗어났습니다. (max: {MainStageData.Count - 1})");
            return;
        }
        var mainStage = MainStageData[mainIndex];
        if (mainStage == null || mainStage.subStages == null)
        {
            Debug.LogWarning($"[SettingDataManager] UnlockStage: MainStageData[{mainIndex}] 또는 subStages가 null입니다.");
            return;
        }
        if (subIndex < 0 || subIndex >= mainStage.subStages.Count)
        {
            Debug.LogWarning($"[SettingDataManager] UnlockStage: subIndex({subIndex})가 범위를 벗어났습니다. (max: {mainStage.subStages.Count - 1})");
            return;
        }
        if (mainStage.subStages[subIndex] == null)
        {
            Debug.LogWarning($"[SettingDataManager] UnlockStage: subStage({mainIndex},{subIndex})가 null입니다.");
            return;
        }
        mainStage.subStages[subIndex].isUnlocked = true;
    }


    public List<List<bool>> SaveClearData()
    {
        List<List<bool>> boolListList = new ();

        for (int i = 0; i < MainStageData.Count; i++)
        {
            List<bool> boolList = new ();

            if (MainStageData[i] == null || MainStageData[i].subStages == null)
            {
                boolListList.Add(boolList);
                continue;
            }

            for(int j = 0; j < MainStageData[i].subStages.Count; j++)
            {
                if (MainStageData[i].subStages[j] == null)
                {
                    boolList.Add(false);
                    continue;
                }
                boolList.Add(MainStageData[i].subStages[j].isUnlocked);
            }

            boolListList.Add(boolList);
        }

        return boolListList;
    }

    public void LoadClearData(List<List<bool>> boolListList)
    {
        if (boolListList == null)
        {
            Debug.LogWarning("[SettingDataManager] LoadClearData: boolListList가 null입니다.");
            return;
        }

        for (int i = 0; i < boolListList.Count; i++)
        {
            // 저장 당시보다 메인 스테이지 수가 적어졌거나, MainStageData가 아직 로드되지 않은 경우 방어
            if (i >= MainStageData.Count || MainStageData[i] == null)
            {
                Debug.LogWarning($"[SettingDataManager] LoadClearData: MainStageData[{i}] 접근 불가. 건너뜁니다.");
                continue;
            }
            if (boolListList[i] == null) continue;

            var subStages = MainStageData[i].subStages;
            if (subStages == null) continue;

            for (int j = 0; j < boolListList[i].Count; j++)
            {
                // 저장 당시보다 서브 스테이지 수가 적어진 경우 방어
                if (j >= subStages.Count || subStages[j] == null)
                {
                    continue;
                }

                bool result = boolListList[i][j];
                if (result)
                {
                    if(j == 0)
                    {
                        MainStageData[i].isUnlocked = true;
                    }


                    subStages[j].isUnlocked = result;
                    Debug.Log($"{i + 1}-{j + 1} 클리어 결과 로드");
                }
            }
        }
    }
}