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
        if (!GameManager.IsTutorialCompleted)
        {
            Debug.Log("튜토리얼 유물 세팅");
            equippedActiveAfData[0] = DataManager.ArtifactData.GetData(08010001); // 튜토리얼용 액티브 유물
            equippedActiveAfData[1] = DataManager.ArtifactData.GetData(08010002); // 튜토리얼용 액티브 유물
            equippedActiveAfData[2] = DataManager.ArtifactData.GetData(08010003); // 튜토리얼용 액티브 유물
            equippedActiveAfData[3] = DataManager.ArtifactData.GetData(08010004); // 튜토리얼용 액티브 유물
            equippedActiveAfData[4] = DataManager.ArtifactData.GetData(08010005); // 튜토리얼용 액티브 유물
            equippedActiveAfData[5] = null;
            equippedActiveAfData[6] = null;
            equippedActiveAfData[7] = null;
        }
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
