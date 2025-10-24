using System;
using System.Collections.Generic;
using UnityEngine;

public class UIUnitSynergeIconArea : MonoBehaviour
{
    [SerializeField] List<UICardSynergyIcon> uICardSynergyIcons = new List<UICardSynergyIcon>();
    // 아이콘 저장 
    Dictionary<UnitSynergyType, Sprite> synergyIconSprites = new Dictionary<UnitSynergyType, Sprite>();
    // 힙 할당 줄이기위한 자료구조
    // enum 배열
    UnitSynergyType[] _allSynergyTypes = (UnitSynergyType[])Enum.GetValues(typeof(UnitSynergyType));
    List<Sprite> unitSynergySprites = new List<Sprite>();
    private void Awake()
    {
        // 스프라이트 미리 로드하기
        foreach (UnitSynergyType type in _allSynergyTypes)
        {
            if (type == UnitSynergyType.None) continue;
            Sprite[] sprites = Resources.LoadAll<Sprite>($"Synergy/{type.ToString()}");
            // 골드 스프라이트만 저장
            synergyIconSprites[type] = sprites[1];
        }
    }
    public void SetUnitSynergeIcon(BaseUnitData data)
    {
        // 비트 플래그 기반으로 모든 시너지 확인
        unitSynergySprites.Clear();
        UnitSynergyType synergyType = data.synergyType;
        foreach (UnitSynergyType type in _allSynergyTypes)
        {
            if ((synergyType & type) != 0)
                unitSynergySprites.Add(synergyIconSprites[type]);
        }
        int idx = unitSynergySprites.Count - 1;
        for(int i = 0; i < uICardSynergyIcons.Count; i++)
        {
            if(idx == i)
            {
                uICardSynergyIcons[i].SetSynergyIcons(unitSynergySprites);
            }
            else
            {
                uICardSynergyIcons[i].gameObject.SetActive(false);
            }
        }
    }
}
