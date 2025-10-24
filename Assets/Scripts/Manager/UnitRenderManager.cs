using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class UnitRenderManager : SingletonMono<UnitRenderManager>
{
    // 싱글톤 인스턴스 접근 안되게 하기
    private new static UnitRenderManager Instance => SingletonMono<UnitRenderManager>.Instance;
    protected override bool IsPersistent => false; // 파괴 가능으로 설정
    Dictionary<PoolType, UnitTextureHandler> UnitRenderTextureByPoolType = new Dictionary<PoolType, UnitTextureHandler>();

    Vector3 startPos = new Vector3(0, -50f, 0); // 카메라 렌더링 방해 안하도록 시작 위치를 멀리 설정
    protected override void Awake()
    {
        base.Awake();
        // 세팅된 플레이어 덱 정보기반으로 매니저 세팅하기
        int activeDeckIndex = PlayerDataManager.Instance.ActiveDeckIndex;
        List<BaseUnitData> deckBaseUnitDatas = PlayerDataManager.Instance.DeckPresets[activeDeckIndex].BaseUnitDatas;
        for (int i = 0; i < deckBaseUnitDatas.Count; i++)
        {
            if (deckBaseUnitDatas[i] == null) // 빈 슬롯은 비활성화
            {
                continue;
            }
            BaseUnitData cardData = deckBaseUnitDatas[i];

            GameObject unitTextureHandlerGO = new GameObject($"UnitTextureHandler_{cardData.poolType.ToString()}");
            unitTextureHandlerGO.transform.SetParent(gameObject.transform);
            unitTextureHandlerGO.transform.localPosition = startPos + new Vector3(4f * i, 0f);

            UnitTextureHandler unitTextureHandler = unitTextureHandlerGO.AddComponent<UnitTextureHandler>();
            unitTextureHandler.Init(cardData.poolType);
            UnitRenderTextureByPoolType[cardData.poolType] = unitTextureHandler;
        }
    }
    protected override void OnDestroy()
    {
        base.OnDestroy();
        foreach (var handler in UnitRenderTextureByPoolType.Values)
        {
            if (handler) Destroy(handler.gameObject);
        }
        UnitRenderTextureByPoolType.Clear();
    }
    public static UnitTextureHandler GetUnitTextureHandlerByPoolType(PoolType poolType)
    {
        if (Instance.UnitRenderTextureByPoolType.TryGetValue(poolType, out UnitTextureHandler handler))
        {
            return handler;
        }
        else
        {
            Debug.LogWarning($"해당 PoolType {poolType}에 대한 UnitTextureHandler가 없습니다.");
            return null;
        }
    }


}
