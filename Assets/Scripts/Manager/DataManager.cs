using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataManager : SingletonMono<DataManager>
{
    private DataBase<ItemData, ItemSO> _itemData;
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

    private DataBase<EnemyData, EnemySO> _enemyData;
    public DataBase<EnemyData, EnemySO> EnemyData
    {
        get
        {
            if(Instance._enemyData == null)
            {
                Instance._enemyData = new DataBase<EnemyData, EnemySO> ();
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
    protected override void Awake()
    {
        base.Awake();
    }
}
