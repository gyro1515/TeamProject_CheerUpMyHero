using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class SettingDataManager : SingletonMono<SettingDataManager>
{
    public List<MainStageData> MainStageData { get; private set; } = new();
    public int ControlPanelLayoutType { get; private set; } // 배틀씬 하단 레이아웃 설정 값 | 0 : 기본값 | 1 : 바꾼 값
    public const string ControlPanelLayoutTypeKey = "ControlPanelLayoutType";
    public static event Action OnControlLayoutChanged;

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
                sb.Append(i+1).Append(0).Append(0).Append(j+1);
                string indexSTr = sb.ToString();
                bool successCast = int.TryParse(indexSTr, out int index);
                MainStageData[i].subStages.Add(DataManager.Instance.SubStageData.GetData(index));
                sb.Clear();
            }
        }
    }

    private void SetLayoutSetting(int type)
    {
        if (ControlPanelLayoutType == type) return;

        ControlPanelLayoutType = type;
        PlayerPrefs.SetInt(ControlPanelLayoutTypeKey, ControlPanelLayoutType);
        PlayerPrefs.Save();

        OnControlLayoutChanged?.Invoke();
    }

    private void LoadLayoutSetting()
    {
        ControlPanelLayoutType =  PlayerPrefs.GetInt(ControlPanelLayoutTypeKey, 0);
    }

    //스테이지 해금 정보


    public void SetStageData()
    {
        
    }

}
