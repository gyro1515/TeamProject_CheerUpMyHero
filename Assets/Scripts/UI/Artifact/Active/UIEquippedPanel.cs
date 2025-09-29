using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIEquippedPanel : MonoBehaviour
{
    [Header("장착 패널 세팅")]
    [SerializeField] List<UIEquippedArtifactSlot> equippedArtifactSlots = new List<UIEquippedArtifactSlot>();
    int maxCnt = 3;
    [SerializeField] List<ActiveAfData> equipData; // 장착 데이터, 현재 확인용
    ActiveAfData selectedAfData = null;
    // 장착할 시 슬롯 세팅하는 용도
    Dictionary<ActiveAfData, UIEquippedArtifactSlot> dataToSlotDic = new Dictionary<ActiveAfData, UIEquippedArtifactSlot>();

    public event Action<ActiveAfData> OnSetOwnedSlotEquipState;
    public event Action<ActiveAfData> OnClickBtn;
    public event Action OnResetOwnedSelectedData;

    bool isInit = false;
    private void OnEnable()
    {
        // 필요없을 거 같지만, 일단은 활성화 될때마다 리셋
        if(isInit) ReSetSlotData();
    }
    private void Start()
    {
        ReSetSlotData();
        isInit = true;
    }
    private void OnDisable()
    {
        ArtifactManager.Instance.EquippedActiveAfData.Clear();
        ArtifactManager.Instance.EquippedActiveAfData.AddRange(equipData);
    }
    public bool EquipActiveArtifact(ActiveAfData data) // 매개 변수로 유물 데이터 있어야 함
    {
        if (equipData.Count >= maxCnt) { Debug.Log("장착 칸이 가득 찼습니다."); return false; }

        data.isEquipped = true;
        int nextIdx = equipData.Count;
        equipData.Add(data);
        equippedArtifactSlots[nextIdx].SetSlot(data, () => 
        { 
            OnClickedSlotBtn(equippedArtifactSlots[nextIdx]); 
        });
        dataToSlotDic[data] = equippedArtifactSlots[nextIdx];
        return true;
    }
    public void UnEquipActiveArtifact(ActiveAfData data) // 매개 변수로 유물 데이터 있어야 함
    {
        if(!equipData.Contains(data))
        {
            Debug.LogWarning("버그 발생, 장착 데이터 없음");
            return;
        }
        data.isEquipped = false;
        equipData.Remove(data);
        dataToSlotDic.Clear();
        selectedAfData = null;
        ReSetSlotData();
    }
    public void AutoAssignAfByList(List<ActiveAfData> sortedAfData)
    {
        RemoveAllEquippedActiveArtifact();
        equipData = sortedAfData;
        for (int i = 0; i < equipData.Count; i++)
        {
            equipData[i].isEquipped = true;
        }
        ReSetSlotData();
    }
    public void RemoveAllEquippedActiveArtifact()
    {
        //Debug.Log("일괄 해제");
        for (int i = 0; i < equipData.Count; i++)
        {
            equipData[i].isEquipped = false;
            OnSetOwnedSlotEquipState?.Invoke(equipData[i]); // 보유 유물 패널의 슬롯도 갱신
        }
        equipData.Clear();
        selectedAfData = null;
        dataToSlotDic.Clear();
        ReSetSlotData();
    }
    void OnClickedSlotBtn(UIEquippedArtifactSlot slot)
    {
        // 소유 유물 선택 해제
        OnResetOwnedSelectedData?.Invoke();
        // 기존 아웃라인 끄기
        if (selectedAfData != null) dataToSlotDic[selectedAfData].SetOutLine(false);
        if (selectedAfData == slot.ActiveAfData) // 재클릭 시
        {
            selectedAfData = null;
        }
        else
        {
            selectedAfData = slot.ActiveAfData;
            dataToSlotDic[selectedAfData].SetOutLine(true);
        }
        // 설명 패널, 장착/해제 버튼 세팅
        OnClickBtn?.Invoke(selectedAfData);
    }
    public void ResetSelectedAfData()
    {
        if (selectedAfData == null) return;
        dataToSlotDic[selectedAfData].SetOutLine(false);
        selectedAfData = null;
    }
    void ReSetSlotData()
    {
        for (int i = 0; i < equippedArtifactSlots.Count; i++)
        {
            if (i < equipData.Count) // 데이터 있다면
            {
                int tmpIdx = i; // C# 람다 캡처(closure capture) 문제 
                equippedArtifactSlots[i].SetSlot(equipData[i], () => 
                { 
                    OnClickedSlotBtn(equippedArtifactSlots[tmpIdx]); 
                });
                dataToSlotDic[equipData[tmpIdx]] = equippedArtifactSlots[tmpIdx];
            }
            else
            {
                equippedArtifactSlots[i].SetSlot(null);
            }
        }
    }
}
