//using UnityEngine;
//using System;
//using System.Collections.Generic;


//플레이어데이터매니저로 이동

//public class ResourceManager : SingletonMono<ResourceManager>
//{
//    // 특정 자원의 수량 변경을 알리는 이벤트
//    public event Action<ResourceType, int> OnResourceChangedEvent;

//    // 각 자원 타입과 수량을 저장할 딕셔너리
//    private Dictionary<ResourceType, int> _resources = new();

//    protected override void Awake()
//    {
//        base.Awake();

//        if (Instance == this)
//        {
//            InitializeResources();
//        }
//    }

//    private void InitializeResources()
//    {
//        // 5가지 자원을 모두 딕셔너리에 추가하고 초기 수량을 설정.
//        _resources[ResourceType.Gold] = 1000;
//        _resources[ResourceType.Wood] = 1000;
//        _resources[ResourceType.Iron] = 1000;
//        _resources[ResourceType.Food] = 1000;
//        _resources[ResourceType.MagicStone] = 1000;
//    }

//    // 특정 자원의 현재 수량을 반환하는 메서드
//    public int GetResourceAmount(ResourceType type)
//    {
//        if (_resources.TryGetValue(type, out int amount))
//        {
//            return amount;
//        }
//        Debug.LogWarning($"ResourceManager: 존재하지 않는 자원 타입입니다. ({type})");
//        return -1;
//    }

//    // 특정 자원의 수량을 변경하는 메서드
//    public void AddResource(ResourceType type, int amount)
//    {
//        if (_resources.ContainsKey(type))
//        {
//            _resources[type] += amount;
//            OnResourceChangedEvent?.Invoke(type, _resources[type]); //자원 수량 변경 이벤트
//        }
//        else
//        {
//            Debug.LogWarning($"ResourceManager: 존재하지 않는 자원 타입입니다. ({type})");
//        }
//    }
//}