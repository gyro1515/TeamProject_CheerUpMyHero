using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArtifactManager : SingletonMono<ArtifactManager>
{
    public event Action OnEquippedArtifactChanged;

    public List<ArtifactData> OwnedArtifacts { get; private set; } = new List<ArtifactData>();       // 플레이어가 보유 중인 유물 리스트

    private const int ArtifactSlotCount = 8;
    public List<ArtifactData> EquippedArtifacts { get; private set; } = new List<ArtifactData>();   // 플레이어가 장착한 유물 리스트

    protected override void Awake()
    {
        base.Awake();

        LoadArtifactData();

        SetAfDataForTest(); // 추후 삭제 예정***********

        // 패시브 유물 테스트 -----
        AddArtifact(080200015);
        AddArtifact(080200014);
        AddArtifact(080200025);
        AddArtifact(080200024);
        AddArtifact(080200035);
        AddArtifact(080200034);
        AddArtifact(080200055);
        AddArtifact(080200054);
        AddArtifact(080200054);
        AddArtifact(080200085);
        AddArtifact(080200084);
        // ------------------------
    }

    private void OnEnable()
    {
        
    }

    private void OnDisable()
    {
        
    }

    // 플레이어 소유에 유물 추가하는 메서드
    public void AddArtifact(int id)
    {
        if (DataManager.Instance.ArtifactData.TryGetValue(id, out ArtifactData data))
        {
            OwnedArtifacts.Add(data);
        }
        else
        {
            Debug.Log("유물 id null이거나 뭔가 문제 있어요 점검하기");
        }
    }

    // 유물 장착하는 메서드 -> 중복 체크하고 slotIndex번째 리스트 슬롯에 데이터 넣어줌.
    public void EquipArtifact(ArtifactData artifact, int slotIndex)
    {
        if (artifact == null) return;
        if (slotIndex < 0 || slotIndex >= ArtifactSlotCount) return;

        for (int i = 0; i < ArtifactSlotCount; i++)
        {
            if (i != slotIndex && EquippedArtifacts[i] != null && EquippedArtifacts[i].name == artifact.name) return;
        }

        EquippedArtifacts[slotIndex] = artifact;
        OnEquippedArtifactChanged?.Invoke();
    }

    // 유물 장착 해제하는 메서드
    public void UnEquipArtifact(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= ArtifactSlotCount) return;
        if (EquippedArtifacts[slotIndex] == null) return;

        EquippedArtifacts[slotIndex] = null;
        OnEquippedArtifactChanged?.Invoke();
    }
    
    // 장착한 패시브 유물의 특정 스탯 타입 값 도출하는 메서드 -> 패시브 유물인 지 확인하고 계산함
    public float GetPassiveArtifactStatBonus(EffectTarget target, StatType statType)
    {
        float totalBonus = 0f;

        foreach (ArtifactData artifact in EquippedArtifacts)
        {
            if (artifact is PassiveArtifactData passiveAf)
            {
                if (passiveAf.effectTarget == target && passiveAf.statType == statType)
                {
                    totalBonus += passiveAf.value;
                }
            }
        }
        return totalBonus;
    }

    // 특정 패시브 아티팩트 id로 값 얻어오는 메서드
    public float GetPassiveArtifactDataValue(int idNumber)
    {
        if (DataManager.Instance.ArtifactData.TryGetValue(idNumber, out ArtifactData data))
        {
            if (data is PassiveArtifactData passiveArtifactData)
            {
                return passiveArtifactData.value;
            }
        }
        return 0f;
    }

    public void LoadArtifactData()
    {
        // 저장된 데이터 불러오는 로직 넣기~~~~ 지금은 못 넣음~~~~~

        bool hasSaveData = false;

        if (hasSaveData)
        {
            // 저장 데이터 불러오는 거 넣기~~~
        }
        else    // 아예 게임 처음이면 초기화 메서드 
        {
            InitializeEquippedArtifacts();
        }
    }

    private void InitializeEquippedArtifacts()      // 유물 초기화 메서드 -> 없으면 NullReference 생기더라구요
    {
        EquippedArtifacts = new List<ArtifactData>(new ArtifactData[ArtifactSlotCount]);
    }

    // 소유 액티브 유물 데이터
    public List<ActiveAfData> OwnedActiveAfData { get; private set; } = new List<ActiveAfData>();
    // 장착 액티브 유물 데이터
    public List<ActiveAfData> EquippedActiveAfData { get; private set; } = new List<ActiveAfData>();
    void SetAfDataForTest() // 추후 삭제 예정***********
    {
        // 테스트 데이터 세팅, 우선 15개
        for (int i = 0; i < 15; i++)
        {
            ActiveAfData data = new ActiveAfData();
            data.name = $"데이터{i + 1}";
            data.lv = UnityEngine.Random.Range(1, 100);
            int desMul = UnityEngine.Random.Range(3, 31);
            string description = "";
            for (int j = 0; j < desMul; j++)
            {
                description += "설명 ";
            }
            data.description = description;
            data.cooldown = UnityEngine.Random.Range(30, 251);
            data.type = UnityEngine.Random.Range(0, 2) > 1 ? "공격" : "디버프";
            data.cost = UnityEngine.Random.Range(3, 11);
            OwnedActiveAfData.Add(data);
        }
    }
}
