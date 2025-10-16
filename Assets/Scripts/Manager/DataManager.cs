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
    public DataBase<ArtifactData, ArtifactSO> ArtifactData
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

    protected override void Awake()
    {
        base.Awake();
    }
}