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


    public TileDataHandler()
    {
        BuildingGridData = new BuildingUpgradeData[5, 5];
        TileStatusGrid = new TileStatus[5, 5];
        TileRepairTurnsGrid = new int[5, 5];

        for (int y = 0; y < 5; y++)
        {
            for (int x = 0; x < 5; x++)
            {
                TileStatusGrid[x, y] = TileStatus.Normal;
                TileRepairTurnsGrid[x, y] = 0;
            }
        }
    }
    public void SwapBuildingData(int sourceX, int sourceY, int destX, int destY)
    {
        var temp = BuildingGridData[destX, destY];
        BuildingGridData[destX, destY] = BuildingGridData[sourceX, sourceY];
        BuildingGridData[sourceX, sourceY] = temp;

        Debug.Log($"건물 위치 교체: ({sourceX},{sourceY}) <-> ({destX},{destY})");
        EventManager.Publish(new GridStateChangedEvent());
    }
    public void MoveBuildingData(int sourceX, int sourceY, int destX, int destY)
    {
        if (BuildingGridData[destX, destY] == null)
        {
            BuildingGridData[destX, destY] = BuildingGridData[sourceX, sourceY];
            BuildingGridData[sourceX, sourceY] = null;

            Debug.Log($"건물 위치 이동: ({sourceX},{sourceY}) -> ({destX},{destY})");
            EventManager.Publish(new GridStateChangedEvent());
        }
    }
    public void CalculateTotalBuildingEffects(
    out int bonusMaxFood,
    out float foodGainPercent,
    out float cooldownReduction,
    out int rareSlots,
    out int epicSlots,
    Dictionary<(int, int), float> tileEfficiencyBonuses)
    {
        bonusMaxFood = 0;
        foodGainPercent = 0f;
        cooldownReduction = 0f;
        rareSlots = 0;
        epicSlots = 0;

        for (int y = 0; y < 5; y++)
        {
            for (int x = 0; x < 5; x++)
            {
                if (TileStatusGrid[x, y] != TileStatus.Normal) continue;
                var building = BuildingGridData[x, y];
                if (building == null) continue;

                //이 타일의 기본 효율을 1.0 (100%)로 시작
                float efficiencyMultiplier = 1.0f; // '고정값' 효과에 사용할 곱셈 보너스
                float additiveBonusPercent = 0f;

                //만약 이 타일에 대한 지역 보너스가 있다면, 효율에 더해줌
                if (tileEfficiencyBonuses.TryGetValue((x, y), out float bonusPercent))
                {
                    efficiencyMultiplier += bonusPercent / 100.0f; 
                    additiveBonusPercent = bonusPercent; // 덧셈용으로 퍼센트 값 저장
                }

                foreach (var effect in building.effects)
                {
                    switch (effect.effectType)
                    {
                        case BuildingEffectType.MaximumFood:
                            if (building.buildingType == BuildingType.Farm) bonusMaxFood += Mathf.CeilToInt(effect.effectValueMin * efficiencyMultiplier);
                            break;
                        case BuildingEffectType.IncreaseFoodGainSpeed:
                            if (building.buildingType == BuildingType.Farm) foodGainPercent += effect.effectValueMin + additiveBonusPercent;
                            break;
                        case BuildingEffectType.UnitCoolDown:
                            if (building.buildingType == BuildingType.Barracks) cooldownReduction += effect.effectValueMin * additiveBonusPercent;
                            break;

                        case BuildingEffectType.CanSummonRareUnits:
                            if (building.buildingType == BuildingType.Barracks) rareSlots += (int)effect.effectValueMin;
                            break;
                        case BuildingEffectType.CanSummonEpicUnits:
                            if (building.buildingType == BuildingType.Barracks) epicSlots += (int)effect.effectValueMin;
                            break;
                    }
                }
            }
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
        EventManager.Publish(new GridStateChangedEvent());
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
            EventManager.Publish(new GridStateChangedEvent());
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

}