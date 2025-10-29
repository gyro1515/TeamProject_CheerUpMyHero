using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEngine;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;

[ExcelAsset(AssetPath = "Resources/DB")]
public class PlayerUnitSO : MonoSO<BaseUnitData>
{
	public List<HeroData> hero_unit;
	public List<BaseUnitData> hiller_unit; 
	public List<BaseUnitData> allianceCommon; 
	public List<BaseUnitData> allianceRare; 
	public List<BaseUnitData> allianceEpic;
	public List<BaseUnitData> spawnedUnit;

    public override List<BaseUnitData> GetList()
    {
        throw new NotImplementedException();
    }

    public override void SetData(Dictionary<int, BaseUnitData> DB)
    {
        for (int i = 0; i < hero_unit.Count; i++)
        {
            var data = hero_unit[i];
            if (data == null) continue;

            // 풀타입과 아이디 넘버 둘 다로 접근 가능하게 설정
            // 둘 다 가능한 이유는 풀타입 크기는 아이디 넘버 최소값보다 항상 작기 때문
            // ex) 풀타입 개수 99개, 아이디 넘버 최소값 100000
            // 나중에 풀타입 개수가 100000이상이라면 문제가 되겠지만 그럴 일은 없을 것이라 예상
            // 따라서 안전하게 둘 다로 접근 가능하게 설정
            // 아이디 넘버는 최대 8자리까지 이므로, 추후 최소 아이디 넘버를 10000000 이상으로 설정한다면 더 확대 가능
            DB[(int)data.poolType] = data;
            DB[data.idNumber] = data;
            hero_unit[i].firstWaveSprite = Resources.Load<Sprite>(data.firstWaveSpritePath);
            hero_unit[i].spawnSprite = Resources.Load<Sprite>(data.spawnSpritePath);
        }
        /*for (int i = 0; i < hiller_unit.Count; i++)
        {
            var data = hiller_unit[i];
            if (data == null) continue;

            // 풀타입과 아이디 넘버 둘 다로 접근 가능하게 설정
            DB[(int)data.poolType] = data;
            DB[data.idNumber] = data;
            data.unitBGSprite = SetBgFromSynergy(data.synergyType);
            data.unitIconSprite = Resources.Load<Sprite>($"UnitIcon/{data.poolType.ToString()}");
            data.gachaHeroSprite = Resources.Load<Sprite>($"GachaHero/{data.poolType.ToString()}");
        }*/
        for (int i = 0; i < allianceCommon.Count; i++)
        {
            var data = allianceCommon[i];
            if (data == null) continue;

            // 풀타입과 아이디 넘버 둘 다로 접근 가능하게 설정
            DB[(int)data.poolType] = data;
            DB[data.idNumber] = data;
            data.unitBGSprite = SetBgFromSynergy(data.synergyType);
            data.unitIconSprite = Resources.Load<Sprite>($"UnitIcon/{data.poolType.ToString()}");
            data.gachaHeroSprite = Resources.Load<Sprite>($"GachaHero/{data.poolType.ToString()}");
        }
        for (int i = 0; i < allianceRare.Count; i++)
        {
            var data = allianceRare[i];
            if (data == null) continue;

            // 풀타입과 아이디 넘버 둘 다로 접근 가능하게 설정
            DB[(int)data.poolType] = data;
            DB[data.idNumber] = data;
            data.unitBGSprite = SetBgFromSynergy(data.synergyType);
            data.unitIconSprite = Resources.Load<Sprite>($"UnitIcon/{data.poolType.ToString()}");
            data.gachaHeroSprite = Resources.Load<Sprite>($"GachaHero/{data.poolType.ToString()}");
        }
        for (int i = 0; i < allianceEpic.Count; i++)
        {
            var data = allianceEpic[i];
            if (data == null) continue;

            // 풀타입과 아이디 넘버 둘 다로 접근 가능하게 설정
            DB[(int)data.poolType] = data;
            DB[data.idNumber] = data;
            data.unitBGSprite = SetBgFromSynergy(data.synergyType);
            data.unitIconSprite = Resources.Load<Sprite>($"UnitIcon/{data.poolType.ToString()}");
            data.gachaHeroSprite = Resources.Load<Sprite>($"GachaHero/{data.poolType.ToString()}");
        }
        for (int i = 0; i < spawnedUnit.Count; i++)
        {
            var data = spawnedUnit[i];
            if (data == null) continue;

            // 풀타입과 아이디 넘버 둘 다로 접근 가능하게 설정
            DB[(int)data.poolType] = data;
            DB[data.idNumber] = data;
        }
    }
    Sprite SetBgFromSynergy(UnitSynergyType synergyType)
    {
        // 시트 모두 불러오기
        Sprite[] allSlices1 = Resources.LoadAll<Sprite>("Synergy/Synergy_Background1");
        Sprite[] allSlices2 = Resources.LoadAll<Sprite>("Synergy/Synergy_Background2");

        // 1순위
        for(int i = (int)UnitSynergyType.Kingdom; i <= (int)UnitSynergyType.Empire; i = i << 1)
        {
            // 비트 체크
            if (((int)synergyType & i) == 0) continue; // 꺼져있으면 패스

            // enum 변환
            UnitSynergyType tmpType = (UnitSynergyType)i;
            return System.Array.Find(allSlices1, s => s.name == tmpType.ToString());
        }
        // 2순위
        for (int i = (int)UnitSynergyType.Mage; i <= (int)UnitSynergyType.Hero; i = i << 1)
        {
            // 비트 체크
            if (((int)synergyType & i) == 0) continue; // 꺼져있으면 패스

            // enum 변환
            UnitSynergyType tmpType = (UnitSynergyType)i;
            return System.Array.Find(allSlices1, s => s.name == tmpType.ToString());
        }
        // 3순위
        for (int i = (int)UnitSynergyType.Frost; i <= (int)UnitSynergyType.Poison; i = i << 1)
        {
            // 비트 체크
            if (((int)synergyType & i) == 0) continue; // 꺼져있으면 패스

            // enum 변환
            UnitSynergyType tmpType = (UnitSynergyType)i;
            return System.Array.Find(allSlices2, s => s.name == tmpType.ToString());
        }


        // 만약 allSlices1, allSlices2가 통일되어 있다면 아래 코드로 대체 가능
        /*foreach (UnitSynergyType type in Enum.GetValues(typeof(UnitSynergyType)))
        {
            if (synergyType.HasFlag(type))
            {
                return System.Array.Find(allSlices1, s => s.name == type.ToString());
            }
        }*/

        return null;
    }
}
