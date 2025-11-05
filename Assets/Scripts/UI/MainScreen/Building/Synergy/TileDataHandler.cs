using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class TileDataHandler
{
    // --- 영지 그리드 데이터 ---
    public BuildingUpgradeData[,] BuildingGridData { get; set; }
    public TileStatus[,] TileStatusGrid { get; private set; }
    public int[,] TileRepairTurnsGrid { get; private set; }
    public DateTime[,] CooldownEndTimeGrid { get; private set; }

    IEventPublisher<GridStateChangedEvent> onGridStateChangedEventPub;
    public TileDataHandler()
    {
        BuildingGridData = new BuildingUpgradeData[5, 5];
        TileStatusGrid = new TileStatus[5, 5];
        TileRepairTurnsGrid = new int[5, 5];
        CooldownEndTimeGrid = new DateTime[5, 5];

        for (int y = 0; y < 5; y++)
        {
            for (int x = 0; x < 5; x++)
            {
                TileStatusGrid[x, y] = TileStatus.Normal;
                TileRepairTurnsGrid[x, y] = 0;
                CooldownEndTimeGrid[x, y] = DateTime.MinValue;
            }
        }
        onGridStateChangedEventPub = EventManager.GetPublisher<GridStateChangedEvent>();
    }
    public void SwapBuildingData(int sourceX, int sourceY, int destX, int destY)
    {
        var temp = BuildingGridData[destX, destY];
        BuildingGridData[destX, destY] = BuildingGridData[sourceX, sourceY];
        BuildingGridData[sourceX, sourceY] = temp;

        var tempCooldown = CooldownEndTimeGrid[destX, destY];
        CooldownEndTimeGrid[destX, destY] = CooldownEndTimeGrid[sourceX, sourceY];
        CooldownEndTimeGrid[sourceX, sourceY] = tempCooldown;

        Debug.Log($"건물 위치 교체: ({sourceX},{sourceY}) <-> ({destX},{destY})");
        onGridStateChangedEventPub?.Publish();
    }
    public void MoveBuildingData(int sourceX, int sourceY, int destX, int destY)
    {
        if (BuildingGridData[destX, destY] == null)
        {
            BuildingGridData[destX, destY] = BuildingGridData[sourceX, sourceY];
            BuildingGridData[sourceX, sourceY] = null;

            CooldownEndTimeGrid[destX, destY] = CooldownEndTimeGrid[sourceX, sourceY];
            CooldownEndTimeGrid[sourceX, sourceY] = DateTime.MinValue; // 원래 위치는 초기화

            Debug.Log($"건물 위치 이동: ({sourceX},{sourceY}) -> ({destX},{destY})");
            onGridStateChangedEventPub?.Publish();
        }
    }
    public void StartCooldownForBuildingAt(int x, int y)
    {
        var building = BuildingGridData[x, y];
        if (building == null || building.level <= 0) return;

        var cooldownDuration = TimeSpan.FromMinutes(building.level * 3);
        CooldownEndTimeGrid[x, y] = DateTime.UtcNow + cooldownDuration;

        Debug.Log($"[쿨타임] ({x},{y}) 타일의 {building.buildingName} 쿨타임 시작.");
    }
    public void CalculateTotalBuildingEffects(
    out int bonusMaxFood,
    out float foodGainPercent,
    out float cooldownReduction,
    out int rareSlots,
    out int epicSlots,
    // tileEfficiencyBonuses는 시너지 시스템에서 계산된 '입력' 값으로 가정합니다.
    Dictionary<(int, int), float> tileEfficiencyBonuses)
    {
        // 1. 모든 'out' 변수 초기화
        bonusMaxFood = 0;
        foodGainPercent = 0f;
        cooldownReduction = 0f;
        rareSlots = 0;
        epicSlots = 0;

        // --- 2. 최고 레벨 병영을 찾기 위한 임시 변수 ---
        BuildingUpgradeData highestLevelBarracks = null;

        // --- 3. '모든' 건물을 순회하며 '중첩' 효과를 계산하고, '최고 병영'을 찾습니다 ---
        for (int y = 0; y < 5; y++)
        {
            for (int x = 0; x < 5; x++)
            {
                var building = BuildingGridData[x, y];
                // 건물이 없거나, 반파/수리 중이면 이 타일은 무시
                if (building == null || TileStatusGrid[x, y] != TileStatus.Normal) continue;

                // --- 3-1. 이 건물이 '병영'이라면, 최고 레벨인지 확인 ---
                if (building.buildingType == BuildingType.Barracks)
                {
                    if (highestLevelBarracks == null || building.level > highestLevelBarracks.level)
                    {
                        highestLevelBarracks = building; // 최고 레벨 병영으로 기록
                    }
                }

                // --- 3-2. '중첩'되는 효과들을 계산합니다 ---
                float efficiencyMultiplier = 1.0f;
                float additiveBonusPercent = 0f;

                if (tileEfficiencyBonuses.TryGetValue((x, y), out float bonusPercent))
                {
                    efficiencyMultiplier += bonusPercent / 100.0f;
                    additiveBonusPercent = bonusPercent;
                }

                foreach (var effect in building.effects)
                {
                    switch (effect.effectType)
                    {
                        // --- 중첩되는 효과들 (+=) ---
                        case BuildingEffectType.MaximumFood:
                            if (building.buildingType == BuildingType.Farm)
                                bonusMaxFood += Mathf.CeilToInt(effect.effectValueMin * efficiencyMultiplier);
                            break;
                        case BuildingEffectType.IncreaseFoodGainSpeed:
                            if (building.buildingType == BuildingType.Farm)
                                foodGainPercent += effect.effectValueMin + additiveBonusPercent;
                            break;
                        case BuildingEffectType.UnitCoolDown:
                            if (building.buildingType == BuildingType.Barracks)
                                cooldownReduction += effect.effectValueMin + additiveBonusPercent; 
                            break;

                        // --- 비-중첩 효과들 (여기서는 무시) ---
                        case BuildingEffectType.CanSummonRareUnits:
                        case BuildingEffectType.CanSummonEpicUnits:
                            // 이 효과들은 루프가 끝난 후 'highestLevelBarracks'로만 계산합니다.
                            break;
                    }
                }
            }
        }

        // --- 4.'최고 레벨 병영'의 '비-중첩' 효과를 적용합니다 ---
        if (highestLevelBarracks != null)
        {
            Debug.Log($"[TileDataHandler] 최고 레벨 병영(Lv.{highestLevelBarracks.level}) 발견. 슬롯 효과 적용.");

            // '최고 레벨 병영'의 효과 목록만 다시 순회합니다.
            foreach (var effect in highestLevelBarracks.effects)
            {
                switch (effect.effectType)
                {
                    //  '=' (할당)을 사용하여 덮어씁니다 (중첩X)
                    case BuildingEffectType.CanSummonRareUnits:
                        rareSlots = (int)effect.effectValueMin;
                        break;
                    case BuildingEffectType.CanSummonEpicUnits:
                        epicSlots = (int)effect.effectValueMin;
                        break;
                }
            }
        }
        else
        {
            Debug.Log("[TileDataHandler] 활성화된 병영 건물이 없어 유닛 슬롯이 0입니다.");
            // (rareSlots와 epicSlots는 이미 0으로 초기화됨)
        }
    }
    public void ReduceCooldownForBuildingAt(int x, int y, int minutesToReduce)
    {
        DateTime currentEndTime = CooldownEndTimeGrid[x, y];

        if (currentEndTime > DateTime.UtcNow)
        {
            var reduction = TimeSpan.FromMinutes(minutesToReduce);
            CooldownEndTimeGrid[x, y] -= reduction;
            Debug.Log($"({x},{y}) 타일의 쿨타임이 {minutesToReduce}분 감소되었습니다.");
        }
    }
public void DamageRandomTile()
    {
        List<(int x, int y)> availableTiles = new List<(int, int)>();
        for (int y = 0; y < 5; y++)
        {
            for (int x = 0; x < 5; x++)
            {
                bool isSpecial = (x == 4 || y == 4);
                if (!isSpecial && TileStatusGrid[x, y] == TileStatus.Normal)
                {
                    availableTiles.Add((x, y));
                }
            }
        }

        if (availableTiles.Count == 0)
        {
            Debug.Log("더 이상 파괴할 수 있는 타일이 없습니다.");
            return;
        }

        int randomIndex = Random.Range(0, availableTiles.Count);
        (int randomX, int randomY) = availableTiles[randomIndex];

        TileStatusGrid[randomX, randomY] = TileStatus.Damaged;
        TileRepairTurnsGrid[randomX, randomY] = 3;

        if (BuildingGridData[randomX, randomY] != null)
        {
            Debug.Log($"패배 페널티: ({randomX}, {randomY}) 타일의 건물이 '반파'되었습니다.");
        }
        else
        {
            Debug.Log($"패배 페널티: ({randomX}, {randomY}) 타일이 '황폐화'되었습니다.");
        }
        onGridStateChangedEventPub?.Publish();
    }

    public void AdvanceRepairTurn()
    {
        bool wasAnyTileRepaired = false;
        for (int y = 0; y < 5; y++)
        {
            for (int x = 0; x < 5; x++)
            {
                bool isWasted = (TileStatusGrid[x, y] == TileStatus.Damaged && BuildingGridData[x, y] == null);
                bool isRepairing = (TileStatusGrid[x, y] == TileStatus.Repairing);

                if (isWasted || isRepairing)
                {
                    if (TileRepairTurnsGrid[x, y] > 0)
                    {
                        TileRepairTurnsGrid[x, y]--;

                        if (TileRepairTurnsGrid[x, y] == 0)
                        {
                            TileStatusGrid[x, y] = TileStatus.Normal;
                            Debug.Log($"타일 ({x},{y})이(가) 자동으로 수리 완료되었습니다.");
                            wasAnyTileRepaired = true;
                        }
                    }
                }
            }
        }

        if (wasAnyTileRepaired)
        {
            onGridStateChangedEventPub?.Publish();
        }
    }
    public List<DetectedSynergy> DetectAllSynergies()
    {
        var detectedSynergies = new List<DetectedSynergy>();
        var usedTiles = new bool[5, 5]; // 시너지에 이미 포함된 타일을 추적하여 중복 방지

        // 우선순위 1: 라인 시너지 (4칸)
        DetectLineSynergies(detectedSynergies, usedTiles);

        // 우선순위 2: 블록 시너지 (2x2)
        DetectBlockSynergies(detectedSynergies, usedTiles);

        // 우선순위 3: 인접 시너지 (2칸)
        DetectAdjacencySynergies(detectedSynergies, usedTiles);

        return detectedSynergies;
    }

    // --- 시너지 감지 헬퍼 메서드 ---

    private BuildingType GetBuildingTypeAt(int x, int y)
    {
        if (x < 0 || x >= 4 || y < 0 || y >= 4) return BuildingType.None; // 일반 타일(4x4) 범위를 벗어나면 없음 처리
        if (TileStatusGrid[x, y] != TileStatus.Normal) return BuildingType.None;

        return BuildingGridData[x, y]?.buildingType ?? BuildingType.None;
    }

    private void DetectLineSynergies(List<DetectedSynergy> detected, bool[,] used)
    {
        for (int y = 0; y < 4; y++)
        {
            BuildingType firstType = GetBuildingTypeAt(0, y);
            if (firstType != BuildingType.None && firstType == GetBuildingTypeAt(1, y) && firstType == GetBuildingTypeAt(2, y) && firstType == GetBuildingTypeAt(3, y))
            {
                if (used[0, y] || used[1, y] || used[2, y] || used[3, y]) continue;
                if (GetLineSynergyType(firstType) is BuildingSynergyType lineSynergy)
                {
                    var positions = new List<(int, int)> { (0, y), (1, y), (2, y), (3, y) };
                    detected.Add(new DetectedSynergy(lineSynergy, positions));
                    positions.ForEach(p => used[p.Item1, p.Item2] = true);
                }
            }
        }
        for (int x = 0; x < 4; x++)
        {
            BuildingType firstType = GetBuildingTypeAt(x, 0);
            if (firstType != BuildingType.None && firstType == GetBuildingTypeAt(x, 1) && firstType == GetBuildingTypeAt(x, 2) && firstType == GetBuildingTypeAt(x, 3))
            {
                if (used[x, 0] || used[x, 1] || used[x, 2] || used[x, 3]) continue;
                if (GetLineSynergyType(firstType) is BuildingSynergyType lineSynergy)
                {
                    var positions = new List<(int, int)> { (x, 0), (x, 1), (x, 2), (x, 3) };
                    detected.Add(new DetectedSynergy(lineSynergy, positions));
                    positions.ForEach(p => used[p.Item1, p.Item2] = true);
                }
            }
        }
    }

    private void DetectBlockSynergies(List<DetectedSynergy> detected, bool[,] used)
    {
        for (int y = 0; y < 3; y++) { for (int x = 0; x < 3; x++) { if (used[x, y] || used[x + 1, y] || used[x, y + 1] || used[x + 1, y + 1]) continue; var types = new HashSet<BuildingType> { GetBuildingTypeAt(x, y), GetBuildingTypeAt(x + 1, y), GetBuildingTypeAt(x, y + 1), GetBuildingTypeAt(x + 1, y + 1) }; if (types.Contains(BuildingType.None)) continue; var pos = new List<(int, int)> { (x, y), (x + 1, y), (x, y + 1), (x + 1, y + 1) }; if (types.Count == 1) { detected.Add(new DetectedSynergy(BuildingSynergyType.Specialized_Block, pos)); pos.ForEach(p => used[p.Item1, p.Item2] = true); } else if (types.Count == 4 && types.IsSupersetOf(new[] { BuildingType.Farm, BuildingType.LumberMill, BuildingType.Mine, BuildingType.Barracks })) { detected.Add(new DetectedSynergy(BuildingSynergyType.Balanced_Block, pos)); pos.ForEach(p => used[p.Item1, p.Item2] = true); } } }
    }

    private void DetectAdjacencySynergies(List<DetectedSynergy> detected, bool[,] used)
    {
        for (int y = 0; y < 4; y++)
        {
            for (int x = 0; x < 4; x++)
            {
                if (used[x, y]) continue;
                var currentType = GetBuildingTypeAt(x, y);
                if (currentType == BuildingType.None) continue;

                if (x < 4 && !used[x + 1, y])
                {
                    if (GetAdjacencySynergyType(currentType, GetBuildingTypeAt(x + 1, y)) is BuildingSynergyType synergy)
                    {
                        var pos = new List<(int, int)> { (x, y), (x + 1, y) };
                        detected.Add(new DetectedSynergy(synergy, pos));
                        used[x, y] = true; used[x + 1, y] = true;
                        continue;
                    }
                }

                if (y < 4 && !used[x, y + 1])
                {
                    if (GetAdjacencySynergyType(currentType, GetBuildingTypeAt(x, y + 1)) is BuildingSynergyType synergy)
                    {
                        var pos = new List<(int, int)> { (x, y), (x, y + 1) };
                        detected.Add(new DetectedSynergy(synergy, pos));
                        used[x, y] = true; used[x, y + 1] = true;
                    }
                }
            }
        }
    }

    // --- 타입 매핑 헬퍼 ---
    private BuildingSynergyType? GetLineSynergyType(BuildingType type) => type switch
    {
        BuildingType.Farm => BuildingSynergyType.Farm_Line,
        BuildingType.LumberMill => BuildingSynergyType.LumberMill_Line,
        BuildingType.Mine => BuildingSynergyType.Mine_Line,
        BuildingType.Barracks => BuildingSynergyType.Barracks_Line,
        _ => null
    };

    private BuildingSynergyType? GetAdjacencySynergyType(BuildingType type1, BuildingType type2)
    {
        var types = new HashSet<BuildingType> { type1, type2 };
        if (types.Contains(BuildingType.None)) return null;

        if (types.SetEquals(new[] { BuildingType.Farm, BuildingType.Barracks })) return BuildingSynergyType.Farm_Barracks;
        if (types.SetEquals(new[] { BuildingType.Barracks, BuildingType.Mine })) return BuildingSynergyType.Barracks_Mine;
        if (types.SetEquals(new[] { BuildingType.Barracks, BuildingType.LumberMill })) return BuildingSynergyType.Barracks_LumberMill;
        if (types.SetEquals(new[] { BuildingType.Mine, BuildingType.LumberMill })) return BuildingSynergyType.Mine_LumberMill;
        if (types.SetEquals(new[] { BuildingType.Farm, BuildingType.Mine })) return BuildingSynergyType.Farm_Mine;
        if (types.SetEquals(new[] { BuildingType.Farm, BuildingType.LumberMill })) return BuildingSynergyType.Farm_LumberMill;
        return null;
    }

    public TileDataSnapshot GetSnapshot()
    {
        var snapshot = new TileDataSnapshot();

        for (int y = 0; y < 5; y++)
        {
            for (int x = 0; x < 5; x++)
            {
                // 1. 건물 데이터 -> 건물 ID로 변환
                // 건물이 없으면 -1 또는 0과 같은 Null 대체 값을 저장합니다. 여기서는 0을 사용.
                snapshot.BuildingIdGrid[x, y] = BuildingGridData[x, y]?.idNumber ?? 0;

                // 2. 타일 상태 및 수리 턴 복사
                snapshot.TileStatusGrid[x, y] = this.TileStatusGrid[x, y];
                snapshot.TileRepairTurnsGrid[x, y] = this.TileRepairTurnsGrid[x, y];

                // 3. DateTime -> long으로 변환
                snapshot.CooldownEndTimeBinaryGrid[x, y] = this.CooldownEndTimeGrid[x, y].ToBinary();
            }
        }

        Debug.Log("TileDataHandler 스냅샷 생성 완료.");
        return snapshot;
    }

    public void RestoreFromSnapshot(TileDataSnapshot snapshot)
    {
        if (snapshot == null)
        {
            Debug.LogWarning("복원할 TileDataSnapshot이 null입니다.");
            return;
        }

        for (int y = 0; y < 5; y++)
        {
            for (int x = 0; x < 5; x++)
            {
                // 1. 건물 ID -> 건물 데이터(참조)로 복원
                int buildingId = snapshot.BuildingIdGrid[x, y];
                if (buildingId > 0)
                {
                    // DataManager를 통해 ID에 해당하는 실제 건물 데이터를 가져옵니다.
                    this.BuildingGridData[x, y] = DataManager.Instance.BuildingUpgradeData.GetData(buildingId);
                }
                else
                {
                    this.BuildingGridData[x, y] = null;
                }

                // 2. 타일 상태 및 수리 턴 복원
                this.TileStatusGrid[x, y] = snapshot.TileStatusGrid[x, y];
                this.TileRepairTurnsGrid[x, y] = snapshot.TileRepairTurnsGrid[x, y];

                // 3. long -> DateTime으로 복원
                this.CooldownEndTimeGrid[x, y] = DateTime.FromBinary(snapshot.CooldownEndTimeBinaryGrid[x, y]);
            }
        }

        // 중요: 상태가 변경되었으므로, 외부(PlayerDataManager)에서 시너지 재계산을 유도하기 위해 이벤트를 발행합니다.
        onGridStateChangedEventPub?.Publish();
        Debug.Log("TileDataHandler 스냅샷으로부터 데이터 복원 완료.");
    }
}

//저장가능한 데이터로 만들어주기
[System.Serializable]
public class TileDataSnapshot
{
    // 1. 건물 ID 그리드 (BuildingUpgradeData -> int)
    public int[,] BuildingIdGrid;

    // 2. 타일 상태 그리드 (Enum은 직렬화 가능)
    public TileStatus[,] TileStatusGrid;

    // 3. 타일 수리 턴 그리드
    public int[,] TileRepairTurnsGrid;

    // 4. 쿨타임 종료 시각 그리드 (DateTime -> long)
    public long[,] CooldownEndTimeBinaryGrid;

    // 생성자: 배열 크기를 초기화해야 직렬화/역직렬화 시 오류가 발생하지 않습니다.
    public TileDataSnapshot()
    {
        // 5x5 그리드에 맞게 초기화
        BuildingIdGrid = new int[5, 5];
        TileStatusGrid = new TileStatus[5, 5];
        TileRepairTurnsGrid = new int[5, 5];
        CooldownEndTimeBinaryGrid = new long[5, 5];
    }
}