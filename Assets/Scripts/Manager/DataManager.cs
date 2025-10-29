using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class DataManager : SingletonMono<DataManager>
{
    /* private DataBase<ItemData, ItemSO> _itemData;
    public DataBase<ItemData,ItemSO> ItemData
    {
        get
        {
            if(Instance._itemData == null)
            {
                Instance._itemData = new DataBase<ItemData, ItemSO>();
            }
            return Instance._itemData;
        }
    } 

    public ItemSO ItemSO => ItemData.SO; */

    private DataBase<ArtifactData, ArtifactSO> _artifactData;
    public static DataBase<ArtifactData, ArtifactSO> ArtifactData
    {
        get
        {
            if (Instance._artifactData == null)
            {
                Instance._artifactData = new DataBase<ArtifactData, ArtifactSO>();
            }
            return Instance._artifactData;
        }
    }

    private DataBase<EnemyData, EnemySO> _enemyData;
    public DataBase<EnemyData, EnemySO> EnemyData
    {
        get
        {
            if (Instance._enemyData == null)
            {
                Instance._enemyData = new DataBase<EnemyData, EnemySO>();
            }
            return Instance._enemyData;
        }
    }

    private DataBase<StageRewardData, StageRewardSO> _rewardData;
    public DataBase<StageRewardData, StageRewardSO> RewardData
    {
        get
        {
            if (Instance._rewardData == null)
            {
                Instance._rewardData = new DataBase<StageRewardData, StageRewardSO>();
            }
            return Instance._rewardData;
        }
    }

    private DataBase<BuildingUpgradeData, BuildingUpgradeSO> _buildingUpgradeData;
    public DataBase<BuildingUpgradeData, BuildingUpgradeSO> BuildingUpgradeData
    {
        get
        {
            if (Instance._buildingUpgradeData == null)
            {
                Instance._buildingUpgradeData = new DataBase<BuildingUpgradeData, BuildingUpgradeSO>();
            }
            return Instance._buildingUpgradeData;
        }
    }

    private DataBase<MainStageData, MainStageSO> _mainStageData;
    public DataBase<MainStageData, MainStageSO> MainStageData
    {
        get
        {
            if (Instance._mainStageData == null)
            {
                Instance._mainStageData = new DataBase<MainStageData, MainStageSO>();
            }
            return Instance._mainStageData;
        }
    }

    private DataBase<SubStageData, SubStageSO> _subStageData;
    public DataBase<SubStageData, SubStageSO> SubStageData
    {
        get
        {
            if (Instance._subStageData == null)
            {
                Instance._subStageData = new DataBase<SubStageData, SubStageSO>();
            }
            return Instance._subStageData;
        }
    }
    private DataBase<StageWaveData, StageWaveSO> _stageWaveData;
    public DataBase<StageWaveData, StageWaveSO> StageWaveData
    {
        get
        {
            if (Instance._stageWaveData == null)
            {
                Instance._stageWaveData = new DataBase<StageWaveData, StageWaveSO>();
            }
            return Instance._stageWaveData;
        }
    }

    private DataBase<StageModifierData, StageModifierSO> _stageModifierData;
    public DataBase<StageModifierData, StageModifierSO> StageModifierData
    {
        get
        {
            if (Instance._stageModifierData == null)
            {
                Instance._stageModifierData = new DataBase<StageModifierData, StageModifierSO>();
            }
            return Instance._stageModifierData;
        }
    }
    private DataBase<BaseUnitData, EnemyUnitSO> _enemyUnitData;
    public static DataBase<BaseUnitData, EnemyUnitSO> EnemyUnitData
    {
        get
        {
            if (Instance._enemyUnitData == null)
            {
                Instance._enemyUnitData = new DataBase<BaseUnitData, EnemyUnitSO>();
            }
            return Instance._enemyUnitData;
        }
    }
    private DataBase<BaseUnitData, PlayerUnitSO> _playerUnitData;
    public static DataBase<BaseUnitData, PlayerUnitSO> PlayerUnitData
    {
        get
        {
            if (Instance._playerUnitData == null)
            {
                Instance._playerUnitData = new DataBase<BaseUnitData, PlayerUnitSO>();
            }
            return Instance._playerUnitData;
        }
    }
    private DataBase<PlayerData, PlayerSO> _playerData;
    public static DataBase<PlayerData, PlayerSO> PlayerData
    {
        get
        {
            if (Instance._playerData == null)
            {
                Instance._playerData = new DataBase<PlayerData, PlayerSO>();
            }
            return Instance._playerData;
        }
    }

    // 유닛 카드 시너지에 사용할 아이콘 저장(골드만) 
    //public Dictionary<UnitSynergyType, Sprite> SynergyIconSprites { get; private set; } = new Dictionary<UnitSynergyType, Sprite>();
    // 유닛 카드 타입(딜/힐탱)에 사용할 아이콘
    public Dictionary<UnitType, Sprite> UnitTypeIconSprites { get; private set; } = new Dictionary<UnitType, Sprite>();
    // 배틀씬/카드덱 선택에서 사용할 아이콘 저장(브론즈, 골드, 프리즘 전부)
    public Dictionary<(UnitSynergyType, SynergyGrade), Sprite> SynergyIconSprites { get; private set; } = new Dictionary<(UnitSynergyType, SynergyGrade), Sprite>();
    // 유닛 시너지 툴팁 설명 데이터
    private DataBase<SynergyData, SynergyEffectSO> _synergyEffectData;
    public static DataBase<SynergyData, SynergyEffectSO> SynergyEffectData
    {
        get
        {
            if (Instance._synergyEffectData == null)
            {
                Instance._synergyEffectData = new DataBase<SynergyData, SynergyEffectSO>();
            }
            return Instance._synergyEffectData;
        }
    }


    //오디오 데이터
    AudioData audioData;  // 오디오 클립 담아둔 오디오 데이터
    public static AudioData AudioData { get => Instance.audioData; }
    protected override void Awake()
    {
        base.Awake();
        // 데이터 베이스가 아닌 데이터들도 여기서 추가 로딩 가능합니다.
        // 시너지 아이콘 스프라이트
        MakeSynergyIconSpritesData();
        // 유닛 타입 아이콘 스프라이트
        MakeUnitTypeIconSpritesData();
        // 오디오 데이터 로드
        audioData = Resources.Load<AudioData>("Sound/SoundData");
    }
    void MakeSynergyIconSpritesData()
    {
        //스프라이트 미리 로드하기
        foreach (UnitSynergyType type in (UnitSynergyType[]) Enum.GetValues(typeof(UnitSynergyType)))
        {
            if (type == UnitSynergyType.None) continue;
            Sprite[] sprites = Resources.LoadAll<Sprite>($"Synergy/{type.ToString()}");
            for(int i = 0; i < sprites.Length; i++)
            {
                // 브론즈, 골드, 프리즘 스프라이트 저장
                SynergyIconSprites[(type, (SynergyGrade)i)] = sprites[i];
            }
            
            // 골드 스프라이트만 저장
            //SynergyIconSprites[type] = sprites[1];
        }
    }
    void MakeUnitTypeIconSpritesData()
    {
        Sprite[] sprites = Resources.LoadAll<Sprite>($"Icon/Position");
        foreach (UnitType type in (UnitType[])Enum.GetValues(typeof(UnitType)))
        {
            Sprite sprite = Array.Find(sprites, s => s.name == type.ToString());
            UnitTypeIconSprites[type] = sprite;
        }
    }
}