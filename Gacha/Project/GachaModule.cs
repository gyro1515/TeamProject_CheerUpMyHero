using System;
using System.Collections.Generic;
using Unity.Services.CloudCode.Core;

namespace Gacha;

public enum Rarity
{
    Epic,
    Rare,
    Common
}

public class RarityInfo
{
    public Rarity rarity { get; set; }
    public int Weight { get; set; }
    public int MinIndex { get; set; }
    public int MaxIndex { get; set; }
}

public class GachaModule
{
    //확률표 세팅
    private readonly List<RarityInfo> _rarityTable = new List<RarityInfo>
    {
        new RarityInfo { rarity = Rarity.Epic, Weight = 25, MinIndex = 125001, MaxIndex = 125003 },
        new RarityInfo { rarity = Rarity.Rare, Weight = 180, MinIndex = 115001, MaxIndex = 115005 },
        new RarityInfo { rarity = Rarity.Common, Weight = 795, MinIndex = 105001, MaxIndex = 105010 }
    };

    [CloudCodeFunction("DrawGachaItem")]
    public int DrawGachaItem()
    {
        //등급 결정
        RarityInfo selectedRarity = SelectRarity();

        //ID 선택
        int selectedItemId = SelectItemId(selectedRarity);

        return selectedItemId;
    }

    [CloudCodeFunction("DrawGachaItemTen")]
    public List<int> DrawGachaItemTen()
    {
        List<int> result = new List<int>();

        for (int i = 0; i < 10; i++)
        {
            RarityInfo selectedRarity = SelectRarity();

            int selectedItemId = SelectItemId(selectedRarity);

            result.Add(selectedItemId);
        }

        return result;
    }

    private RarityInfo SelectRarity()
    {
        int totalWeight = 0;
        foreach (var rarity in _rarityTable)
        {
            totalWeight += rarity.Weight;
        }

        Random rand = new Random();
        double randomValue = rand.NextDouble() * totalWeight;

        double cumulativeWeight = 0;
        foreach (var rarityInfo in _rarityTable)
        {
            cumulativeWeight += rarityInfo.Weight;
            if (randomValue < cumulativeWeight)
            {
                return rarityInfo;
            }
        }

        //오류로 선택 안될 경우, 마지막 common 반환
        return _rarityTable[_rarityTable.Count -1];
    }

    // 3단계에서 구현한 함수
    private int SelectItemId(RarityInfo selectedRarity)
    {
        Random rand = new Random();
        return rand.Next(selectedRarity.MinIndex, selectedRarity.MaxIndex + 1);
    }


}
