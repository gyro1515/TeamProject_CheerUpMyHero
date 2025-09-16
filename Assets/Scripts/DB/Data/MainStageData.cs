using System.Collections.Generic;

[System.Serializable]
public class MainStageData : MonoData
{
    public string displayName;          // "1. 겨울왕국"
    public bool isUnlocked = false;     // 메인 스테이지 해금 여부
    public List<SubStageData> subStages = new List<SubStageData>(); // 서브 스테이지 리스트
    public int subStageCount;           // 서브 스테이지 개수
}
