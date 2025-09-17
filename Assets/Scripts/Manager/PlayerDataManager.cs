using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public enum ResourceType
{
    Gold,
    Wood,
    Iron,
    Food,
    MagicStone
}

public class PlayerDataManager : SingletonMono<PlayerDataManager>
{
    protected override void Awake()
    {
        base.Awake();

        if (Instance == this)
        {
            InitializeResources();
        }
    }

    //덱 편성 관련
    #region Deck
    //편성된 덱 정보
    public List<int> DeckList { get; private set; } = new();

    public void SetDeckList(List<int> deckList)
    {
        DeckList.Clear();
        DeckList = deckList;
        StringBuilder sb = new StringBuilder();

        //
        Debug.Log($"[PlayerDataManager] 현재 덱리스트 크기: {DeckList.Count}");

        for (int i = 0; i < DeckList.Count; i++)
        {
            sb.Append(DeckList[i].ToString());
            sb.Append(", ");
        }
        if (sb.Length < 2)
        {
            Debug.Log($"[PlayerDataManager] 덱 세팅 완료 안내메시지를 호출 실패했습니다");
            return;
        }

        sb.Length -= 2;
        Debug.Log($"[PlayerDataManager] 덱 세팅 완료: {sb.ToString()}");
    }
    #endregion

    //자원 관련
    #region Resources
    //
    // 특정 자원의 수량 변경을 알리는 이벤트
    public event Action<ResourceType, int> OnResourceChangedEvent;

    // 각 자원 타입과 수량을 저장할 딕셔너리
    private Dictionary<ResourceType, int> _resources = new();

    private void InitializeResources()
    {
        // 5가지 자원을 모두 딕셔너리에 추가하고 초기 수량을 설정.
        _resources[ResourceType.Gold] = 10000;
        _resources[ResourceType.Wood] = 10000;
        _resources[ResourceType.Iron] = 10000;
        _resources[ResourceType.Food] = 10000;
        _resources[ResourceType.MagicStone] = 10000;
    }

    // 특정 자원의 현재 수량을 반환하는 메서드
    public int GetResourceAmount(ResourceType type)
    {
        if (_resources.TryGetValue(type, out int amount))
        {
            return amount;
        }
        Debug.LogWarning($"ResourceManager: 존재하지 않는 자원 타입입니다. ({type})");
        return -1;
    }

    // 특정 자원의 수량을 변경하는 메서드
    public void AddResource(ResourceType type, int amount)
    {
        if (_resources.ContainsKey(type))
        {
            _resources[type] += amount;
            OnResourceChangedEvent?.Invoke(type, _resources[type]); //자원 수량 변경 이벤트
        }
        else
        {
            Debug.LogWarning($"ResourceManager: 존재하지 않는 자원 타입입니다. ({type})");
        }
    }
    #endregion
}
