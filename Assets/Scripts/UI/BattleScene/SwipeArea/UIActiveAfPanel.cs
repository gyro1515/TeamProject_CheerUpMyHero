using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIActiveAfPanel : MonoBehaviour
{
    [Header("액티브 유물 패널 세팅")]
    [SerializeField] List<UIActiveAFSlot> afSlotList = new List<UIActiveAFSlot>();
    List<ArtifactData> equippedActiveAfData;

    private void Awake()
    {
        // 플레이어 데이터에 따라 슬롯 초기화
        equippedActiveAfData = ArtifactManager.Instance.EquippedArtifacts;
        for (int i = 0; i < afSlotList.Count; i++)
        {
            if (i < equippedActiveAfData.Count)
            {
                afSlotList[i].InitAfSlot(equippedActiveAfData[i]);
            }
            else
            {
                afSlotList[i].InitAfSlot(null); // 빈 슬롯으로 초기화
            }
        }
    }
}
