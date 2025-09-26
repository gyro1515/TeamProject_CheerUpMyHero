using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIActiveAfPanel : MonoBehaviour
{
    [Header("액티브 유물 패널 세팅")]
    [SerializeField] List<UIActiveAFSlot> afSlotList = new List<UIActiveAFSlot>();
    private void Awake()
    {
        // 플레이어 데이터에 따라 슬롯 초기화

    }
}
