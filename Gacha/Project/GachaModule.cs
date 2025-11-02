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
    public List<int> IDs { get; set; } = new List<int>();
}

public class GachaModule
{
    //확률표 세팅
    private readonly List<RarityInfo> _rarityTable = new List<RarityInfo>
    {
        new RarityInfo { rarity = Rarity.Epic, Weight = 25, IDs = new List<int>(){120003, 125001, 125002, 125003 } }, // 120001, 120002 막음
        new RarityInfo { rarity = Rarity.Rare, Weight = 180, IDs = new List<int>(){110001, 110002, 110003, 110004, 110005, 110006, 110007, 115001, 115003, 115004, 115005 } }, //115002 막음
        new RarityInfo { rarity = Rarity.Common, Weight = 795, IDs = new List<int>(){105001, 105002, 105003, 105004, 105005, 105006, 105007, 105008, 105009, 105010 } }
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
        int index = rand.Next(0, selectedRarity.IDs.Count);
        return selectedRarity.IDs[index];
    }

    [CloudCodeFunction("DrawPickUPItem")]
    public int DrawPickUPItem()
    {
        //등급 결정
        RarityInfo selectedRarity = SelectRarity();

        //ID 선택
        int selectedItemId = SelectItemId(selectedRarity);

        if (selectedRarity.rarity == Rarity.Epic)
        {
            Random rand = new Random();
            int num = rand.Next(0, 2);
            if (num == 0)
            {
                selectedItemId = 125001;
            }
        }

        return selectedItemId;
    }

}
