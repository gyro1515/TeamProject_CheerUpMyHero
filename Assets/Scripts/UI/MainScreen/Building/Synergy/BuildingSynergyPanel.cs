using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class BuildingSynergyPanel : MonoBehaviour
{
    [Header("UI 연결")]
    [SerializeField] private Transform scrollContent;
    //[SerializeField] private SynergyInfoItem itemPrefab; // 정보 표시용 아이템 프리팹
    private RectTransform scrollContentRect;
    // 건물 타입에 맞는 아이콘(Sprite)을 저장해두는 딕셔너리
    private Dictionary<BuildingType, Sprite> buildingIcons = new Dictionary<BuildingType, Sprite>();
    private List<SynergyInfoItem> activeItems = new List<SynergyInfoItem>();

    void Awake()
    {
        LoadBuildingIcons();
        scrollContentRect = scrollContent as RectTransform;
    }

    void OnEnable()
    {
        UpdateDisplay(); // 패널이 켜질 때 UI 갱신
    }

    void OnDisable()
    {

    }


    // UI 표시를 업데이트하는 메인 함수
    public void UpdateDisplay()
    {
        foreach (var item in activeItems)
        {
            item.ReleaseSelf();
        }
        activeItems.Clear();

        DisplayEffectsByBuildingType();

        DisplayActiveSynergies();

        StartCoroutine(CoRebuildLayout());
    }

    private IEnumerator CoRebuildLayout()
    {
        yield return new WaitForEndOfFrame();

        if (scrollContentRect != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(scrollContentRect);
        }
    }
    private SynergyInfoItem GetNewItem()
    {
        GameObject itemGO = ObjectPoolManager.Instance.Get(PoolType.SynergyInfoItem);
        itemGO.transform.SetParent(scrollContent, false);
        itemGO.transform.localScale = Vector3.one;

        itemGO.transform.SetAsLastSibling();

        SynergyInfoItem item = itemGO.GetComponent<SynergyInfoItem>();
        activeItems.Add(item);
        return item;
    }
    // --- 세부 UI 표시 함수들 ---

    // 건물 타입별 효과를 합산하여 표시하는 함수
    void DisplayEffectsByBuildingType()
    {
        if (PlayerDataManager.Instance._TileDataHandler == null) return;

        // --- 1. StringBuilder를 함수 맨 위에 선언 (재활용) ---
        var sb = new StringBuilder();

        // 2. '병영'을 제외한 건물들을 타입별로 그룹화
        var buildingsByType = PlayerDataManager.Instance._TileDataHandler.BuildingGridData
            .Cast<BuildingUpgradeData>()
            .Where(b => b != null && b.effects.Count > 0
                       && b.buildingType != BuildingType.Barracks) // 병영 제외
            .GroupBy(b => b.buildingType);

        foreach (var group in buildingsByType)
        {
            var item = GetNewItem();

            var effectsSum = new Dictionary<BuildingEffectType, float>();
            var magicStoneEffects = new List<BuildingEffect>();
            // ------------------------------------

            foreach (var building in group)
            {
                foreach (var effect in building.effects)
                {
                    if (effect.effectType == BuildingEffectType.MagicStoneProduction)
                    {
                        magicStoneEffects.Add(effect);
                    }
                    else
                    {
                        if (!effectsSum.ContainsKey(effect.effectType)) effectsSum[effect.effectType] = 0;
                        effectsSum[effect.effectType] += effect.effectValueMin;
                    }
                }
            }

            sb.Clear(); // StringBuilder 내용 비우기

            foreach (var pair in effectsSum)
            {
                var tempEffect = new BuildingEffect { effectType = pair.Key, effectValueMin = pair.Value };
                string effectString = FormatEffectString(tempEffect);
                if (!string.IsNullOrEmpty(effectString)) sb.AppendLine(effectString);
            }

            foreach (var stoneEffect in magicStoneEffects.Distinct())
            {
                string effectString = FormatEffectString(stoneEffect);
                if (!string.IsNullOrEmpty(effectString)) sb.AppendLine(effectString);
            }

            Sprite icon = buildingIcons.ContainsKey(group.Key) ? buildingIcons[group.Key] : null;
            string title = $"{group.First().buildingName} x{group.Count()}";
            item.Initialize(icon, title, sb.ToString().TrimEnd());
        }
        int totalBarracksCount = PlayerDataManager.Instance._TileDataHandler.BuildingGridData
            .Cast<BuildingUpgradeData>()
            .Count(b => b != null && b.buildingType == BuildingType.Barracks);

        if (totalBarracksCount > 0)
        {
            var item = GetNewItem();
            sb.Clear(); // StringBuilder 재사용

            // PlayerDataManager에 이미 계산된 최종 값을 가져옵니다.
            int finalRareSlots = PlayerDataManager.Instance.RareUnitSlots;
            int finalEpicSlots = PlayerDataManager.Instance.EpicUnitSlots;
            float finalCooldownReduction = PlayerDataManager.Instance.TotalUnitCooldownReduction;

            // (BuildingEffectType.None 등 임시 타입으로 문자열 포맷팅 함수 호출)
            string cooldownString = FormatEffectString(new BuildingEffect
            {
                effectType = BuildingEffectType.UnitCoolDown,
                effectValueMin = finalCooldownReduction
            });
            string rareString = FormatEffectString(new BuildingEffect
            {
                effectType = BuildingEffectType.CanSummonRareUnits,
                effectValueMin = finalRareSlots
            });
            string epicString = FormatEffectString(new BuildingEffect
            {
                effectType = BuildingEffectType.CanSummonEpicUnits,
                effectValueMin = finalEpicSlots
            });

            // --- 4-3. 원하는 순서대로, 그리고 빈 문자열이 아닐 때만 AppendLine ---
            if (!string.IsNullOrEmpty(cooldownString))
            {
                sb.AppendLine(cooldownString);
            }
            if (!string.IsNullOrEmpty(rareString))
            {
                sb.AppendLine(rareString);
            }
            if (!string.IsNullOrEmpty(epicString))
            {
                sb.AppendLine(epicString);
            }

            // 아이콘 및 타이틀 설정
            Sprite icon = buildingIcons.ContainsKey(BuildingType.Barracks) ? buildingIcons[BuildingType.Barracks] : null;
            string title = $"병영 x{totalBarracksCount}"; // (총 병영 개수 표시)

            item.Initialize(icon, title, sb.ToString().TrimEnd());
        }
    }

    // 활성화된 시너지 효과를 표시하는 함수
    void DisplayActiveSynergies()
    {
        var synergies = PlayerDataManager.Instance.ActiveSynergies;
        if (synergies == null || synergies.Count == 0) return;

        foreach (var synergy in synergies)
        {
            var item = GetNewItem();
            (string title, List<BuildingType> types, string desc) = GetSynergyUIData(synergy.Type);

            List<Sprite> icons;

            if (synergy.Type == BuildingSynergyType.Specialized_Block)
            {
                icons = new List<Sprite>();
                if (synergy.TilePositions != null && synergy.TilePositions.Count > 0)
                {
                    var firstTilePos = synergy.TilePositions[0];

                    BuildingUpgradeData buildingData = PlayerDataManager.Instance._TileDataHandler.BuildingGridData[firstTilePos.x, firstTilePos.y];

                    if (buildingData != null)
                    {
                        BuildingType targetType = buildingData.buildingType;
                        if (buildingIcons.ContainsKey(targetType))
                        {
                            icons.Add(buildingIcons[targetType]);
                        }
                    }
                }
            }
            else 
            {
                icons = types.Select(t => buildingIcons.ContainsKey(t) ? buildingIcons[t] : null)
                             .Where(s => s != null).ToList();
            }
            switch (synergy.Type)
            {
                case BuildingSynergyType.Farm_Line:
                case BuildingSynergyType.LumberMill_Line:
                case BuildingSynergyType.Mine_Line:
                case BuildingSynergyType.Barracks_Line:
                    item.Initialize(synergy.Type, icons, title, desc);
                    break;

                default:
                    item.Initialize(synergy.Type, icons, title, desc);
                    break;
            }
        }
    }
    // --- 헬퍼 함수 (데이터 로드 및 텍스트 변환) ---

    // 게임 시작 시 건물 아이콘들을 미리 로드하는 함수
    void LoadBuildingIcons()
    {
        if (DataManager.Instance == null) return;

        var buildableList = PlayerDataManager.Instance.GetBuildableList();
        foreach (var buildData in buildableList)
        {
            var level1Data = DataManager.Instance.BuildingUpgradeData.GetData(buildData.nextLevel);
            if (level1Data != null && !buildingIcons.ContainsKey(level1Data.buildingType))
            {
                buildingIcons.Add(level1Data.buildingType, level1Data.buildingSprite);
            }
        }
    }

    // 시너지 타입에 맞는 UI 데이터를 (제목, 아이콘 목록, 설명) 형태로 반환하는 함수
    (string title, List<BuildingType> types, string description) GetSynergyUIData(BuildingSynergyType type)
    {
        switch (type)
        {
            case BuildingSynergyType.Farm_Barracks:
                return ("[농장 + 병영]", new List<BuildingType> { BuildingType.Farm, BuildingType.Barracks }, "전투 유닛 생산 쿨타임 -2.5%\n초당 식량 획득량 -2.5%");
            case BuildingSynergyType.Barracks_Mine:
                return ("[병영 + 탄광]", new List<BuildingType> { BuildingType.Barracks, BuildingType.Mine }, "모든 유닛 공격력 +1.5%");
            case BuildingSynergyType.Barracks_LumberMill:
                return ("[병영 + 벌목장]", new List<BuildingType> { BuildingType.Barracks, BuildingType.LumberMill }, "모든 유닛 체력 +1.5%");
            case BuildingSynergyType.Mine_LumberMill:
                return ("[탄광 + 벌목장]", new List<BuildingType> { BuildingType.Mine, BuildingType.LumberMill }, "인접 건물 생산량 +2.5%");
            case BuildingSynergyType.Farm_Mine:
                return ("[농장 + 탄광]", new List<BuildingType> { BuildingType.Farm, BuildingType.Mine }, "인접 농장 생산량 +2.5%");
            case BuildingSynergyType.Farm_LumberMill:
                return ("[농장 + 벌목장]", new List<BuildingType> { BuildingType.Farm, BuildingType.LumberMill }, "인접 농장 생산량 +2.5%");
            case BuildingSynergyType.Farm_Line:
                return ("[농업 단지]", new List<BuildingType> { BuildingType.Farm }, "최대 식량 보유량 +5%\n초당 식량 획득량 +2.5%");
            case BuildingSynergyType.LumberMill_Line:
                return ("[벌목 단지]", new List<BuildingType> { BuildingType.LumberMill }, "모든 업그레이드 목재 비용 -5%");
            case BuildingSynergyType.Mine_Line:
                return ("[광산 단지]", new List<BuildingType> { BuildingType.Mine }, "철괴 비용 -5%\n마력석 비용 -2.5%");
            case BuildingSynergyType.Barracks_Line:
                return ("[훈련 단지]", new List<BuildingType> { BuildingType.Barracks }, "모든 유닛 공격 쿨타임 -10%");
            case BuildingSynergyType.Specialized_Block:
                return ("[전문 기술 단지]", new List<BuildingType>(), "블록 내 건물 효율 +2.5%");
            case BuildingSynergyType.Balanced_Block:
                return ("[균형 발전 지구]", new List<BuildingType> { BuildingType.Farm, BuildingType.LumberMill, BuildingType.Mine, BuildingType.Barracks }, "블록 내 건물 효율 +5%");
            default:
                return (type.ToString(), new List<BuildingType>(), "설명 없음");
        }
    }

    private string GetEffectValueString(BuildingEffect effect)
    {
        switch (effect.effectType)
        {
            case BuildingEffectType.IncreaseFoodGainSpeed:
            case BuildingEffectType.AdditionalWoodProduction:
            case BuildingEffectType.AdditionalIronProduction:
            case BuildingEffectType.MagicStoneFindChance:
            case BuildingEffectType.UnitCoolDown:
                return $"{effect.effectValueMin}%";
        }

        if (effect.effectValueMax > 0 && effect.effectValueMin != effect.effectValueMax)
        {
            return $"{effect.effectValueMin}~{effect.effectValueMax}";
        }
        return effect.effectValueMin.ToString();
    }

    private static readonly Dictionary<BuildingEffectType, string> EffectNames = new()
    {
        { BuildingEffectType.MaximumFood, "최대 식량 보유량" },
        { BuildingEffectType.IncreaseFoodGainSpeed, "초당 식량 획득량" },
        { BuildingEffectType.BaseWoodProduction, "기본 목재 획득량" },
        { BuildingEffectType.AdditionalWoodProduction, "추가 목재 획득량" },
        { BuildingEffectType.BaseIronProduction, "기본 철괴 획득량" },
        { BuildingEffectType.AdditionalIronProduction, "추가 철괴 획득량" },
        { BuildingEffectType.UnitCoolDown, "유닛 생산 쿨타임" },
        { BuildingEffectType.MagicStoneFindChance, "마력석 얻을 확률" },
        { BuildingEffectType.MagicStoneProduction, "마력석 획득량" },
        { BuildingEffectType.CanSummonRareUnits, "레어 유닛 참여 수" },
        { BuildingEffectType.CanSummonEpicUnits, "에픽 유닛 참여 수" }
    };

    private string GetEffectNameInKorean(BuildingEffectType type)
        => EffectNames.TryGetValue(type, out var name) ? name : type.ToString();

    private string FormatEffectString(BuildingEffect effect)
    {
        string effectName = GetEffectNameInKorean(effect.effectType);
        string valueString = GetEffectValueString(effect);
        if (effect.effectValueMin == 0)
        {
            return string.Empty; // 또는 null
        }
        switch (effect.effectType)
        {
            case BuildingEffectType.IncreaseFoodGainSpeed:
            case BuildingEffectType.AdditionalWoodProduction:
            case BuildingEffectType.AdditionalIronProduction:
            case BuildingEffectType.MagicStoneFindChance:
            case BuildingEffectType.CanSummonRareUnits:
            case BuildingEffectType.CanSummonEpicUnits:
                return $"{effectName} +{valueString}";
            case BuildingEffectType.UnitCoolDown:
                return $"{effectName} -{valueString}";
            default:
                return $"{effectName} {valueString}";
        }
    }
}