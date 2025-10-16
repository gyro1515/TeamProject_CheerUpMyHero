using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class BuildingSynergyPanel : MonoBehaviour
{
    [Header("UI 연결")]
    [SerializeField] private Transform scrollContent;
    [SerializeField] private SynergyInfoItem itemPrefab; // 정보 표시용 아이템 프리팹

    // 건물 타입에 맞는 아이콘(Sprite)을 저장해두는 딕셔너리
    private Dictionary<BuildingType, Sprite> buildingIcons = new Dictionary<BuildingType, Sprite>();

    private IEventSubscriber<SynergyDataUpdatedEvent> _synergyDataUpdatedSubscriber;

    void Awake()
    {
        LoadBuildingIcons();
        _synergyDataUpdatedSubscriber = EventManager.GetSubscriber<SynergyDataUpdatedEvent>();
        _synergyDataUpdatedSubscriber.Subscribe(OnSynergyDataUpdated);
    }

    void OnEnable()
    {
        UpdateDisplay(); // 패널이 켜질 때 UI 갱신
    }

    void OnDisable()
    {
        if (_synergyDataUpdatedSubscriber != null)
        {
            _synergyDataUpdatedSubscriber.Unsubscribe(OnSynergyDataUpdated);
        }
    }

    void OnSynergyDataUpdated(SynergyDataUpdatedEvent e)
    {
        UpdateDisplay();
    }

    // UI 표시를 업데이트하는 메인 함수
    void UpdateDisplay()
    {
        foreach (Transform child in scrollContent)
        {
            Destroy(child.gameObject);
        }

        DisplayEffectsByBuildingType();
        DisplayActiveSynergies();
    }

    // --- 세부 UI 표시 함수들 ---

    // 건물 타입별 효과를 합산하여 표시하는 함수
    void DisplayEffectsByBuildingType()
    {
        if (PlayerDataManager.Instance._TileDataHandler == null) return;

        var buildingsByType = PlayerDataManager.Instance._TileDataHandler.BuildingGridData
            .Cast<BuildingUpgradeData>()
            .Where(b => b != null && b.effects.Count > 0)
            .GroupBy(b => b.buildingType);

        foreach (var group in buildingsByType)
        {
            var item = Instantiate(itemPrefab, scrollContent);

            // 같은 종류 건물의 효과들을 모두 합산
            var effectsSum = new Dictionary<BuildingEffectType, float>();
            foreach (var building in group)
            {
                foreach (var effect in building.effects)
                {
                    if (!effectsSum.ContainsKey(effect.effectType)) effectsSum[effect.effectType] = 0;
                    effectsSum[effect.effectType] += effect.effectValueMin;
                }
            }

            // 합산된 효과를 하나의 문자열로 만듦
            var sb = new StringBuilder();
            foreach (var pair in effectsSum)
            {
                string effectString = FormatEffectString(pair.Key, pair.Value);
                if (!string.IsNullOrEmpty(effectString)) sb.AppendLine(effectString);
            }

            // `SynergyInfoItem`에 필요한 데이터 준비
            Sprite icon = buildingIcons.ContainsKey(group.Key) ? buildingIcons[group.Key] : null;
            string title = $"x{group.Count()}";

            // `SynergyInfoItem`의 일반 정보 표시용 함수 호출
            item.Initialize(icon, title, sb.ToString());
        }
    }

    // 활성화된 시너지 효과를 표시하는 함수
    void DisplayActiveSynergies()
    {
        var synergies = PlayerDataManager.Instance.ActiveSynergies;
        if (synergies == null || synergies.Count == 0) return;

        foreach (var synergy in synergies)
        {
            var item = Instantiate(itemPrefab, scrollContent);
            (string title, List<BuildingType> types, string desc) = GetSynergyUIData(synergy.Type);

            List<Sprite> icons;

            if (synergy.Type == BuildingSynergyType.Specialized_Block)
            {
                icons = new List<Sprite>();
                if (synergy.TilePositions.Count > 0)
                {
                    // 블록의 첫 번째 타일 위치를 가져옴
                    var pos = synergy.TilePositions[0];
                    // 해당 위치의 건물 데이터를 가져옴
                    var buildingData = PlayerDataManager.Instance._TileDataHandler.BuildingGridData[pos.x, pos.y];
                    if (buildingData != null && buildingIcons.ContainsKey(buildingData.buildingType))
                    {
                        // 건물 타입에 맞는 아이콘을 추가
                        icons.Add(buildingIcons[buildingData.buildingType]);
                    }
                }
            }
            else // 그 외의 시너지는 기존 방식대로 처리
            {
                icons = types.Select(t => buildingIcons.ContainsKey(t) ? buildingIcons[t] : null)
                             .Where(s => s != null).ToList();
            }

            // `SynergyInfoItem`의 시너지 정보 표시용 함수 호출
            item.Initialize(synergy.Type, icons, title, desc);
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

    // 건물 효과 타입과 값을 받아 최종 UI 문자열로 포맷팅하는 함수
    string FormatEffectString(BuildingEffectType type, float value)
    {
        string name = "";
        string format = "+{0}";
        switch (type)
        {
            case BuildingEffectType.MaximumFood: name = "최대 식량 보유량"; break;
            case BuildingEffectType.IncreaseFoodGainSpeed: name = "초당 식량 획득량"; format = "+{0}%"; break;
            case BuildingEffectType.UnitCoolDown: name = "유닛 생산 쿨타임"; format = "-{0}%"; break;
            case BuildingEffectType.BaseWoodProduction: name = "기본 목재 획득량"; break;
            case BuildingEffectType.AdditionalWoodProduction: name = "추가 목재 획득량"; format = "+{0}%"; break;
            case BuildingEffectType.BaseIronProduction: name = "기본 철괴 획득량"; break;
            case BuildingEffectType.AdditionalIronProduction: name = "추가 철괴 획득량"; format = "+{0}%"; break;
            case BuildingEffectType.MagicStoneFindChance: name = "마력석 얻을 확률"; format = "+{0}%"; break;
            case BuildingEffectType.MagicStoneProduction: name = "마력석 획득량"; format = "+{0}"; break;
            case BuildingEffectType.CanSummonRareUnits: name = "레어 유닛 참여 수"; format = "+{0}%"; break;
            case BuildingEffectType.CanSummonEpicUnits: name = "에픽 유닛 참여 수"; format = "+{0}%"; break;
            default: return ""; 
        }
        return $"{name} {string.Format(format, value)}";
    }
}