using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental;
using UnityEngine;
using Random = UnityEngine.Random;

public class ArtifactManager : SingletonMono<ArtifactManager>
{
    public event Action OnEquippedArtifactChanged;
    public event Action OnOwnedArtifactsChanged;

    public ArtifactSO artifactSO;

    public List<ArtifactData> OwnedArtifacts { get; private set; } = new List<ArtifactData>();       // 플레이어가 보유 중인 유물 리스트

    private const int ArtifactSlotCount = 8;
    public List<ArtifactData> EquippedArtifacts { get; private set; } = new List<ArtifactData>();   // 플레이어가 장착한 유물 리스트

    protected override void Awake()
    {
        base.Awake();

        // LoadArtifactData(); // 외부에서 호출하도록 옮김
        InitializeEquippedArtifacts();
        //SetAfDataForTest(); // 추후 삭제 예정***********

        // 패시브 유물 테스트 ----- // 세이브 없을때만 호출하도록 옮김
        AddArtifact(08010001);
        AddArtifact(08010002);
        AddArtifact(08010003);
        AddArtifact(08010004);
        AddArtifact(08010005);
        //AddArtifact(080200025);
        //AddArtifact(080200024);
        //AddArtifact(080200035);
        //AddArtifact(080200034);
        //AddArtifact(080200055);
        //AddArtifact(080200054);
        //AddArtifact(080200054);
        //AddArtifact(080200085);
        //AddArtifact(080200084);
        //AddArtifact(08010001);
        //AddArtifact(08010002);
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
        if (DataManager.ArtifactData.TryGetValue(id, out ArtifactData data))
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
        AudioManager.PlayOneShot(DataManager.AudioData.artifactEquipSE, 0.8f);
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

    // 유물 전체 장착 해제하는 메서드
    public void UnEquipAllArtifacts()
    {
        bool emptied = false;
        for (int i = 0; i < EquippedArtifacts.Count; i++)
        {
            if (EquippedArtifacts[i] != null)
            {
                EquippedArtifacts[i] = null;
                emptied = true;
            }
        }

        if (emptied) OnEquippedArtifactChanged?.Invoke();
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
        if (DataManager.ArtifactData.TryGetValue(idNumber, out ArtifactData data))
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


    //Newtonsoft.Json 사용해서 패시브, 액티브까지 알아서 구분
    public string SaveArtifactData(List<ArtifactData> data)
    {
        JsonSerializerSettings settings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto
        };

        string json = JsonConvert.SerializeObject(data, Formatting.Indented, settings);
        
        return json;
    }


    public void LoadArtifactData(string ownedList, string equippedList)
    {
        // 저장된 데이터 불러오는 로직 넣기~~~~ 지금은 못 넣음~~~~~

        bool hasSaveData = false;

        if (ownedList != null && equippedList != null)
        {
            hasSaveData = true;
        }

        if (hasSaveData)
        {
            InitializeEquippedArtifacts();
            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto
            };


            OwnedArtifacts = JsonConvert.DeserializeObject<List<ArtifactData>>(ownedList, settings);
            List<ArtifactData> equippedData = JsonConvert.DeserializeObject<List<ArtifactData>>(equippedList, settings);

            for (int i = 0; i < equippedData.Count && i < ArtifactSlotCount; i++)
            {
                if (equippedData[i] != null)
                {
                    EquippedArtifacts[i] = equippedData[i];  // 직접 할당
                    Debug.Log($"[ArtifactManager] Slot {i}에 유물 복원: {equippedData[i].name}");
                }
            }

        }
        else    // 아예 게임 처음이면 초기화 메서드 
        {
            InitializeEquippedArtifacts();

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
        }

        OnEquippedArtifactChanged?.Invoke();
        OnOwnedArtifactsChanged?.Invoke();
    }

    private void InitializeEquippedArtifacts()      // 유물 초기화 메서드 -> 없으면 NullReference 생기더라구요
    {
        EquippedArtifacts = new List<ArtifactData>(new ArtifactData[ArtifactSlotCount]);
    }
    #endregion

    // 유물 자동 장착 메서드
    public void AutoEquipArtifacts(ArtifactType type)
    {
        if (type == ArtifactType.None)
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

        foreach (var artifact in primaryList)
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

        AudioManager.PlayOneShot(DataManager.AudioData.artifactEquipSE);
    }

    // 랜덤 패시브 아티팩트 생성하는 메서드 -> 스테이지 클리어 보상 용도
    public List<PassiveArtifactData> GetRandomPassiveArtifact(int count, int chapter)
    {
        // 챕터 5 이상은 챕터 5 확률표 사용
        int adjustedChapter = Mathf.Min(chapter, 4);

        List<PassiveArtifactData> result = new List<PassiveArtifactData>();
        List<PassiveArtifactData> source = new List<PassiveArtifactData>(artifactSO.passiveArtifacts);

        // 챕터별 등급 확률표 (Common, Rare, Epic, Unique, Legendary 순)
        Dictionary<int, float[]> chapterProbabilities = new Dictionary<int, float[]>
        {
            { 0, new float[] { 89.0f, 9.5f, 1.5f, 0f, 0f } },
            { 1, new float[] { 69.5f, 26.0f, 3.5f, 1.0f, 0f } },
            { 2, new float[] { 49.5f, 41.5f, 7.0f, 1.5f, 0.5f } },
            { 3, new float[] { 34.5f, 50.0f, 9.0f, 5.0f, 1.5f } },
            { 4, new float[] { 25.0f, 52.5f, 12.5f, 7.5f, 2.5f } }
        };

        float[] probabilities = chapterProbabilities[adjustedChapter];

        // count만큼 유물 뽑기
        HashSet<PassiveArtifactData> selectedArtifacts = new HashSet<PassiveArtifactData>();
        int attempts = 0;
        int maxAttempts = count * 100; // 무한 루프 방지

        while (selectedArtifacts.Count < count && attempts < maxAttempts)
        {
            attempts++;

            // 1. 확률에 따라 등급 결정
            PassiveArtifactGrade selectedGrade = DetermineGradeByProbability(probabilities);

            // 2. 해당 등급의 유물들만 필터링
            List<PassiveArtifactData> artifactsOfGrade = source
                .Where(a => a.grade == selectedGrade)
                .ToList();

            if (artifactsOfGrade.Count == 0)
            {
                Debug.LogWarning($"챕터 {adjustedChapter}에서 {selectedGrade} 등급의 유물이 없습니다.");
                continue;
            }

            // 3. 해당 등급 내에서 랜덤 선택 (중복 방지)
            PassiveArtifactData selectedArtifact = artifactsOfGrade[Random.Range(0, artifactsOfGrade.Count)];
            selectedArtifacts.Add(selectedArtifact);
        }

        if (attempts >= maxAttempts)
        {
            Debug.LogError($"유물 선택 최대 시도 횟수 초과. 요청: {count}개, 선택: {selectedArtifacts.Count}개");
        }

        result = selectedArtifacts.ToList();
        return result;
    }    

    private PassiveArtifactGrade DetermineGradeByProbability(float[] probabilities)
    {
        float randomValue = Random.Range(0f, 100f);
        float cumulativeProbability = 0f;

        PassiveArtifactGrade[] grades = new PassiveArtifactGrade[]
        {
            PassiveArtifactGrade.Common,
            PassiveArtifactGrade.Rare,
            PassiveArtifactGrade.Epic,
            PassiveArtifactGrade.Unique,
            PassiveArtifactGrade.Legendary
        };

        for (int i = 0; i < probabilities.Length; i++)
        {
            cumulativeProbability += probabilities[i];
            if (randomValue < cumulativeProbability)
            {
                return grades[i];
            }
        }

        // 만약의 사태에 대비해 Common반환
        return PassiveArtifactGrade.Common;
    }

    public void SortOwnedArtifacts()
    {
        ArtifactManager.Instance.OwnedArtifacts.Sort((a, b) =>
        {
            bool isAActive = a is ActiveArtifactData;
            bool isBActive = b is ActiveArtifactData;

            if (isAActive && !isBActive)    // a는 액티브, b가 패시브면 a를 앞으로 둠.
            {
                return -1;
            }
            if (!isAActive && isBActive)    // b는 액티브, a는 패시브면 b를 앞으로 둠.
            {
                return 1;
            }

            if (!isAActive && !isBActive)    // 둘 다 패시브일 경우 
            {
                PassiveArtifactData passiveA = a as PassiveArtifactData;
                PassiveArtifactData passiveB = b as PassiveArtifactData;
                return passiveB.grade.CompareTo(passiveA.grade);    // 등급으로 비교함
            }

            return a.name.CompareTo(b.name);
        });

        OnOwnedArtifactsChanged?.Invoke();
    }

    public List<ActiveArtifactData> GetRandomActiveArtifact(int count)
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
        return result;
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
