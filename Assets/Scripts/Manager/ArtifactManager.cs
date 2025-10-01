using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class ArtifactManager : SingletonMono<ArtifactManager>
{
    public event Action OnEquippedArtifactChanged;

    public ArtifactSO artifactSO;

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
        AddArtifact(08010001);
        AddArtifact(08010002);
        // ------------------------

        artifactSO = Resources.Load<ArtifactSO>("DB/ArtifactSO");
    }

    private void OnEnable()
    {
        
    }

    private void OnDisable()
    {
        
    }

    #region 유물 : 유물 획득, 장착, 해제 등 필수 메서드
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

    // 유물 장착하는 메서드
    public void EquipArtifact(ArtifactData artifact, int slotIndex)
    {
        if (artifact == null) return;
        if (slotIndex < 0 || slotIndex >= ArtifactSlotCount) return;

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
    #endregion

    #region 유물 : 특정 값 얻어오는 메서드
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
    #endregion

    #region 유물 : 저장 및 초기화 관련
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
    #endregion

    // 유물 자동 장착 메서드
    public void AutoEquipArtifacts(ArtifactType type)
    {
        if (type == null)
        {
            Debug.Log("정렬 유형 선택 안 돼서 정렬 안 됨");
            return;
        }

        var sortedPAf = OwnedArtifacts.OfType<PassiveArtifactData>()
                                                     .OrderByDescending(p => p.grade)
                                                     .ThenBy(p => p.idNumber)
                                                     .ToList();

        var sortedAAf = OwnedArtifacts.OfType<ActiveArtifactData>()
                                                     .OrderBy(a => a.levelData[a.curLevel].coolTime)
                                                     .ToList();

        List<ArtifactData> primaryList;
        List<ArtifactData> subList;

        if (type == ArtifactType.Passive)
        {
            primaryList = sortedPAf.Cast<ArtifactData>().ToList();
            subList = sortedAAf.Cast<ArtifactData>().ToList();
        }
        else
        {
            primaryList = sortedAAf.Cast<ArtifactData>().ToList();
            subList = sortedPAf.Cast<ArtifactData>().ToList();
        }

        for (int i = 0; i < ArtifactSlotCount; i++)
        {
            EquippedArtifacts[i] = null;
        }

        int slotIndex = 0;
        // HashSet<(EffectTarget, StatType)> equippedPassiveTypes = new HashSet<(EffectTarget, StatType)>();

        foreach ( var artifact in primaryList)
        {
            //if (artifact is PassiveArtifactData passive)
            //{
            //if (equippedPassiveTypes.Contains((passive.effectTarget, passive.statType)))
            //{
            //    continue;
            //}
            //equippedPassiveTypes.Add((passive.effectTarget, passive.statType));
            // }

            if (slotIndex >= ArtifactSlotCount) break;
            EquippedArtifacts[slotIndex] = artifact;
            slotIndex++;
        }

        foreach (var artifact in subList)
        {
            //if (artifact is PassiveArtifactData passive)
            //{
            //    if (equippedPassiveTypes.Contains((passive.effectTarget, passive.statType)))
            //    {
            //        continue;
            //    }
            //    equippedPassiveTypes.Add((passive.effectTarget, passive.statType));
            //}

            if (slotIndex >= ArtifactSlotCount) break;
            EquippedArtifacts[slotIndex] = artifact;
            slotIndex++;
        }
        OnEquippedArtifactChanged?.Invoke();
    }

    // 랜덤 패시브 아티팩트 생성하는 메서드 -> 스테이지 클리어 보상 용도
    public List<PassiveArtifactData> GetRandomPassiveArtifact(int count)
    {
        List<PassiveArtifactData> source = new List<PassiveArtifactData>(artifactSO.passiveArtifacts);
        List<PassiveArtifactData> result = new List<PassiveArtifactData>();

        int tmpIdx = 0;
        HashSet<int> usedIdx = new HashSet<int>();
        while (tmpIdx < count)
        {
            int randomNum = Random.Range(0, source.Count);
            if (usedIdx.Contains(randomNum)) continue;
            usedIdx.Add(randomNum);
            result.Add(source[randomNum]);
            tmpIdx++;
        }
        /*for (int i = 0; i < count; i++)
        {
            int randomNum = Random.Range(0, source.Count);
            if(source[randomNum] == null)
            {
                Debug.Log("중복 발생 다시 뽑기");
            }
            result.Add(source[randomNum]);
            source[randomNum] = null;
        }*/
        return result;
    }

    public List <ActiveArtifactData> GetRandomActiveArtifact(int count)
    {
        List<ActiveArtifactData> source = new List<ActiveArtifactData>(artifactSO.activeArtifacts);
        List<ActiveArtifactData> result = new List<ActiveArtifactData>();

        int tmpIdx = 0;
        HashSet<int> usedIdx = new HashSet<int>();
        while (tmpIdx < count)
        {
            int randomNum = Random.Range(0, source.Count);
            if (usedIdx.Contains(randomNum)) continue;
            usedIdx.Add(randomNum);
            result.Add(source[randomNum]);
            tmpIdx++;
        }

        /*for (int i = 0; i < count; i++)
        {
            int randomNum = Random.Range(0, source.Count);
            result.Add(source[randomNum]);
            source[randomNum] = null;
        }*/
        return result ;
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
