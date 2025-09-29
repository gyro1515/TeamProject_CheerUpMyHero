using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Runtime.InteropServices.WindowsRuntime;

// 임시 데이터
[System.Serializable]
public class ActiveAfData
{
    public string name = "";
    public int lv = -1;
    public string description = "";
    public float cooldown = -1f;
    public Sprite icon = null;
    public bool isEquipped = false;
    public string type = "";
    public float cost = -1f;
}

public class UIOwnedPanel : MonoBehaviour
{
    [Header("소유 유물 패널 세팅")]
    [SerializeField] Transform slotsTransform;
    [SerializeField] GameObject OwnedArtifactSlotPrefab;
    [SerializeField] Button autoAssignBtn;
    [SerializeField] List<ActiveAfData> activeAfDatas; // 인스펙터 창에서 확인가능하게
    [SerializeField] List<Sprite> afSprite; // 테스트용 스프라이트 세팅

    List<UIOwnedArtifactSlot> forReuseSlotList = new List<UIOwnedArtifactSlot>(); // 슬롯 재사용 용도
    // 장착할 시 슬롯 세팅하는 용도
    Dictionary<ActiveAfData, UIOwnedArtifactSlot> dataToSlotDic = new Dictionary<ActiveAfData, UIOwnedArtifactSlot>();

    ActiveAfData selectedAfData = null;
    public event Action<ActiveAfData> OnEquipStateChange;
    /*public event Func<ActiveAfData, bool> OnEquip;
    public event Action<ActiveAfData> OnUnEquip;*/
    public event Action<ActiveAfData> OnClickBtn;
    public event Action<List<ActiveAfData>> OnAutoAssign;
    public event Action OnResetEquippedSelectedData;

    bool isInit = false; // 초기화 여부 체크용
    private void Awake()
    {
        autoAssignBtn.onClick.AddListener(AutoAssignAf);
        // 테스트용 데이터 세팅 -> 플레이어 데이터 매니저로 이동
        //SetAfDataForTest();
    }
    private void OnEnable()
    {
        // 슬롯 리세팅하기
        if(isInit) ReSetSlotData();
    }
    private void Start()
    {
        activeAfDatas = ArtifactManager.Instance.OwnedActiveAfData;
        SetAfDataForTest(); // 테스트로 스프라이트 세팅
        ReSetSlotData();
        isInit = true;
    }
    public void SetOwnedSlotEquipState(ActiveAfData data)
    {
        if (data == null) { Debug.LogWarning("호출 오류"); return; }
        if (!dataToSlotDic.ContainsKey(data)) { Debug.LogWarning("데이터 세팅 오류"); return; }

        dataToSlotDic[data].SetEquipState();
        // 장착/해제 버튼도 세팅, 여기서 실행되면 중복될 수 있지만 일단은 여기서...
        OnEquipStateChange?.Invoke(selectedAfData);
    }
    void ReSetSlotData()
    {
        // 우선 기존 슬롯 다 비활성화
        for (int i = 0; i < forReuseSlotList.Count; i++)
        {
            forReuseSlotList[i].gameObject.SetActive(false);
        }
        // 자료 구조도 초기화
        dataToSlotDic.Clear();
        // 보유 액티브 유물에 따라 슬롯 생성/활성화
        for (int i = 0; i < activeAfDatas.Count; i++)
        {
            UIOwnedArtifactSlot slot;
            if (i < forReuseSlotList.Count) // 재사용할 슬롯이 있다면
            {
                slot = forReuseSlotList[i];
            }
            else // 없다면 소환
            {
                slot = Instantiate(OwnedArtifactSlotPrefab, slotsTransform).GetComponent<UIOwnedArtifactSlot>();
                forReuseSlotList.Add(slot);
            }
            // 슬롯 세팅
            slot.SetSlot(activeAfDatas[i], () => OnClickedSlotBtn(slot));
            dataToSlotDic[activeAfDatas[i]] = slot;
        }
    }
    void OnClickedSlotBtn(UIOwnedArtifactSlot slot)
    {
        // 장착 유물 선택 해제
        OnResetEquippedSelectedData?.Invoke();
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
    void AutoAssignAf()
    {
        //Debug.Log("자동 편성");
        // 내림 차순 정렬한 배열 생성, 장착 가능 슬롯 만큼
        int equipSlotCount = 3;
        // 추후 필요하다면 루프 기반 O(N) 방식으로 3개 뽑는 것도 고려해야함**********
        List<ActiveAfData> sortedByCost = activeAfDatas.OrderByDescending(d => d.cost).Take(equipSlotCount).ToList();
        // 편성 데이터 기반으로 장착 패널 세팅(장착 슬롯)
        OnAutoAssign?.Invoke(sortedByCost);
        // 유물 장착 상태에 따라 유물 패널(소유 슬롯) 세팅
        for (int i = 0;i < sortedByCost.Count;i++)
        {
            SetOwnedSlotEquipState(sortedByCost[i]);
        }
    }
    void SetAfDataForTest()
    {
        // 테스트 데이터 세팅, 우선 15개
        for (int i = 0; i < activeAfDatas.Count; i++)
        {
            int rnd = UnityEngine.Random.Range(1, afSprite.Count);
            activeAfDatas[i].icon = afSprite[rnd];
        }
    }
    /*public void SetEquip(bool isEquipped)
    {
        if(selectedAfData == null) { Debug.LogWarning("로직 오류"); return; }
        if(isEquipped) // 현재 선택한 슬롯 장착
        {
            if (!OnEquip.Invoke(selectedAfData)) return; // 장착 불가능 하면 아래 세팅 필요 없음
        }
        else // 해제
        {
            OnUnEquip.Invoke(selectedAfData);
        }
        SetOwnedSlotEquipState(selectedAfData);
        
    }*/
}
