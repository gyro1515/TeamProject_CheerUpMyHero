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
    public void CalculateTotalBuildingEffects(
       out int bonusMaxFood,
       out float foodGainPercent,
       out float cooldownReduction,
       out int rareSlots,
       out int epicSlots)
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
                if (TileStatusGrid[x, y] != TileStatus.Normal)
                {
                    continue;
                }
                var building = BuildingGridData[x, y];
                if (building == null) continue;

                foreach (var effect in building.effects)
                {
                    switch (effect.effectType)
                    {
                        case BuildingEffectType.MaximumFood:
                            if (building.buildingType == BuildingType.Farm)
                                bonusMaxFood += (int)effect.effectValueMin;
                            break;
                        case BuildingEffectType.IncreaseFoodGainSpeed:
                            if (building.buildingType == BuildingType.Farm)
                                foodGainPercent += effect.effectValueMin;
                            break;
                        case BuildingEffectType.UnitCoolDown:
                            if (building.buildingType == BuildingType.Barracks)
                                cooldownReduction += effect.effectValueMin;
                            break;
                        case BuildingEffectType.CanSummonRareUnits:
                            if (building.buildingType == BuildingType.Barracks)
                                rareSlots += (int)effect.effectValueMin;
                            break;
                        case BuildingEffectType.CanSummonEpicUnits:
                            if (building.buildingType == BuildingType.Barracks)
                                epicSlots += (int)effect.effectValueMin;
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
}