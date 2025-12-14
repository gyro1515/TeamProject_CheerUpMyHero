using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ArtifactService
{
    private readonly PlayerDataManager _data;

    // 유물 장착 슬롯 const
    private const int ArtifactSlotCount = 8;

    public ArtifactService(PlayerDataManager data)
    {
        _data = data;
    }

    #region 보유 유물 관련 메서드 (추가, 삭제)
    // id 버전 유물 획득 메서드
    public bool AddArtifact(int id)
    {
        if (DataManager.ArtifactData.TryGetValue(id, out var artifact))
        {
            _data.AddOwnedArtifact(artifact);
            return true;
        }

        Debug.Log($"유물 추가 실패함. {id} 없음");
        return false;
    }

    // data 버전 유물 획득 메서드
    public bool AddArtifact(ArtifactData artifact)
    {
        if (artifact == null)
        {
            Debug.Log($"유물 추가 실패함. {artifact} 없음");
            return false;
        }

        _data.AddOwnedArtifact(artifact);
        return true;
    }

    // 특정 유물 id로 찾아서 여러 개 삭제하는 메서드
    public int RemoveArtifactsByIdNumber(int idNumber, int count)
    {
        int removedCount = 0;

        for (int i = _data.OwnedArtifacts.Count - 1; i >= 0 && removedCount < count; i--)
        {
            if (_data.OwnedArtifacts[i].idNumber == idNumber)
            {
                _data.RemoveOwnedArtifact(_data.OwnedArtifacts[i]);
                removedCount++;
            }
        }

        return removedCount;
    }
    #endregion

    #region 장착 유물 관련 메서드 (장착, 해제)
    // 유물 장착 메서드
    public bool EquipArtifact(ArtifactData artifact, int slotIndex)
    {
        if (artifact == null)
        {
            Debug.Log($"유물 장착 실패함. {artifact} 없음");
            return false;
        }

        if (!IsValidSlotIndex(slotIndex))
        {
            Debug.Log($"유물 장착 실패함. {slotIndex} 이상함");
            return false;
        }

        _data.SetEquippedArtifact(slotIndex, artifact);
        PlayEquipSound();
        return true;
    }

    // 유물 장착 해제 메서드
    public bool UnEquipArtifact(int slotIndex)
    {
        if (!IsValidSlotIndex(slotIndex))
        {
            Debug.Log($"유물 해제 실패함. {slotIndex} 이상함");
            return false;
        }

        if (_data.GetEquippedArtifact(slotIndex) == null)
        {
            return false;
        }

        _data.ClearEquippedSlot(slotIndex);
        return true;
    }

    // 유물 전체 해제 메서드
    public void UnEquipAllArtifacts()
    {
        _data.ClearAllEquippedSlots();
    }
    #endregion

    #region 유물 정렬 관련 메서드 (정렬, 자동장착)
    // 보유 유물 정렬 메서드 : 액티브 -> 패시브 등급 -> 패시브 이름
    public void SortOwnedArtifacts()
    {
        var sortedList = _data.OwnedArtifacts.OrderBy(a => a is ActiveArtifactData ? 0 : 1)
                                             .ThenByDescending(a =>
                                             {
                                                 if (a is PassiveArtifactData passive)
                                                     return (int)passive.grade;
                                                 return int.MaxValue;
                                             })
                                             .ThenBy(a => a.name)
                                             .ToList();

        _data.SetOwnedArtifacts(sortedList);
    }

    // 유물 자동 장착 메서드
    public void AutoEquipArtifacts(ArtifactType type)
    {
        if (type == ArtifactType.None)
        {
            Debug.Log("유물 정렬 유형 없어서 정렬 안됨");
            return;
        }

        // 패시브 : 등급 -> ID 순서
        var sortedPassive = _data.OwnedArtifacts
            .OfType<PassiveArtifactData>()
            .OrderByDescending(p => p.grade)
            .ThenBy(p => p.idNumber)
            .Cast<ArtifactData>()
            .ToList();

        // 액티브 : 쿨타임 순서
        var sortedActive = _data.OwnedArtifacts
            .OfType<ActiveArtifactData>()
            .OrderBy(a => a.levelData[a.curLevel].coolTime)
            .Cast<ArtifactData>()
            .ToList();

        List<ArtifactData> primaryList;
        List<ArtifactData> secondaryList;

        if (type == ArtifactType.Passive)
        {
            primaryList = sortedPassive;
            secondaryList = sortedActive;
        }
        else
        {
            primaryList = sortedActive;
            secondaryList = sortedPassive;
        }

        _data.ClearAllEquippedSlots();

        int slotIdx = 0;

        foreach (var artifact in primaryList)
        {
            if (slotIdx >= ArtifactSlotCount) break;
            _data.SetEquippedArtifact(slotIdx, artifact);
            slotIdx++;
        }

        foreach (var artifact in secondaryList)
        {
            if (slotIdx >= ArtifactSlotCount) break;
            _data.SetEquippedArtifact(slotIdx, artifact);
            slotIdx++;
        }

        PlayEquipSound();
    }
    #endregion

    #region 헬퍼 메서드
    // 슬롯 번호 유효성 검사하는 메서드
    private bool IsValidSlotIndex(int slotIndex)
    {
        return slotIndex >= 0 && slotIndex < ArtifactSlotCount;
    }

    // 유물 장착 효과음 메서드
    private void PlayEquipSound()
    {
        AudioManager.PlayOneShot(DataManager.AudioData.artifactEquipSE, 0.8f);
    }
    #endregion 
}
