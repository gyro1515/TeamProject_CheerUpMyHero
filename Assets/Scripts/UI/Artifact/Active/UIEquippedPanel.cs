using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIEquippedPanel : MonoBehaviour
{
    [Header("장착 패널 세팅")]
    [SerializeField] List<UIEquipedArtifactSlot> quipedArtifactSlots = new List<UIEquipedArtifactSlot>();
    int maxCnt = 3;
    [SerializeField] List<int> equipData = new List<int>(); // 장착 데이터, 테스트로 3개 넣어놓기
    private void OnEnable()
    {
        // 필요없을 거 같지만, 일단은 활성화 될때마다 리셋
        ReSetSlotData();
    }
    public void EquipActiveArtifact(int data) // 매개 변수로 유물 데이터 있어야 함
    {
        if (equipData.Count >= maxCnt) { Debug.Log("장착 칸이 가득 찼습니다."); return; }

        equipData.Add(data);
        quipedArtifactSlots[equipData.Count - 1].SetSlot(null, data, $"임시 이름 {data}");
    }
    public void UnEquipActiveArtifact(int data) // 매개 변수로 유물 데이터 있어야 함
    {
        if(!equipData.Contains(data))
        {
            Debug.LogWarning("버그 발생, 장착 데이터 없음");
            return;
        }
        equipData.Remove(data); 
        ReSetSlotData();
    }
    public void RemoveAllEquipActiveArtifact()
    {
        Debug.Log("일괄 해제");
        equipData.Clear();
        ReSetSlotData();
    }
    void ReSetSlotData()
    {
        for (int i = 0; i < quipedArtifactSlots.Count; i++)
        {
            if (i < equipData.Count) // 데이터 있다면
            {
                quipedArtifactSlots[i].SetSlot(null, equipData[i], $"임시 이름 {i}");
            }
            else
            {
                quipedArtifactSlots[i].SetSlotNull();
            }
        }
    }
}
