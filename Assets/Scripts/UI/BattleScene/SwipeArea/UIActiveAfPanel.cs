using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIActiveAfPanel : MonoBehaviour
{
    [Header("액티브 유물 패널 세팅")]
    [SerializeField] List<UIActiveAFSlot> afSlotList = new List<UIActiveAFSlot>();
    List<ArtifactData> equippedActiveAfData;

    // 튜토리얼에서 지급할 액티브 유물 ID
    private static readonly int[] TutorialActiveArtifactIds = new[]
    {
        08010001, 08010002, 08010003, 08010004, 08010005
    };

    private void Awake()
    {
        var playerData = PlayerDataManager.Instance;

        if (!GameManager.IsTutorialCompleted)
        {
            Debug.Log("튜토리얼 유물 세팅 - PlayerDataManager에 정식 등록");
            EnsureTutorialArtifactsRegistered(playerData);
        }

        // 정식 등록 이후엔 항상 PlayerDataManager의 데이터를 단일 소스로 사용
        equippedActiveAfData = playerData.EquippedArtifacts;

        for (int i = 0; i < afSlotList.Count; i++)
        {
            if (equippedActiveAfData != null && i < equippedActiveAfData.Count)
            {
                afSlotList[i].InitAfSlot(equippedActiveAfData[i]);
            }
            else
            {
                afSlotList[i].InitAfSlot(null); // 빈 슬롯으로 초기화
            }
        }
    }

    // 튜토리얼 유물 5개를 OwnedArtifacts와 EquippedArtifacts에 모두 정식 등록.
    // 이미 보유한 경우엔 중복 추가하지 않음 (재진입 안전).
    private void EnsureTutorialArtifactsRegistered(PlayerDataManager playerData)
    {
        if (playerData == null)
        {
            Debug.LogError("[UIActiveAfPanel] PlayerDataManager가 null입니다. 튜토리얼 유물 세팅 불가.");
            return;
        }

        for (int i = 0; i < TutorialActiveArtifactIds.Length; i++)
        {
            int id = TutorialActiveArtifactIds[i];
            ArtifactData data = DataManager.ArtifactData.GetData(id);

            if (data == null)
            {
                Debug.LogError($"[UIActiveAfPanel] 튜토리얼 유물 데이터 누락. ID: {id}");
                continue;
            }

            // 1) 보유 목록에 없으면 추가 (재진입 시 중복 방지)
            if (!playerData.OwnedArtifacts.Contains(data))
            {
                playerData.AddOwnedArtifact(data);
            }

            // 2) 해당 슬롯이 비어있을 때만 장착 (이미 다른 유물이 있다면 덮어쓰지 않음)
            if (playerData.GetEquippedArtifact(i) == null)
            {
                playerData.SetEquippedArtifact(i, data);
            }
        }
    }
}
