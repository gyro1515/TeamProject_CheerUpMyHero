using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class GachaTable 
{ 
    public Rarity rarity;
    public float probability;
    public int minId;
    public int maxId;
}


public class GachaSystem : MonoBehaviour
{

    // 변조 우려 때문에, 원래 가챠 확률 계산을 서버에서 하는게 맞다고 함. 나중에 통째로 서버로 옯겨야 할수도?

    private List<GachaTable> tables = new List<GachaTable>();

    
    
    private void Awake()
    {
        SetGachaData();
    }


    //현재 수동으로 id 범위 입력중, 나중에 수정 필요(근데 어짜피 서버로 간다면 그때 가서 생각해도 될듯)
    void SetGachaData()
    {
        tables.Add(new GachaTable { rarity = Rarity.common, probability = 79.5f, minId = 105001, maxId = 105010 });
        tables.Add(new GachaTable { rarity = Rarity.rare, probability = 18.0f, minId = 115001, maxId = 115005 });
        tables.Add(new GachaTable { rarity = Rarity.epic, probability = 2.5f, minId = 125001, maxId = 125003 });
    }




    //레어도 정하기
    GachaTable GetRarity()
    {
        float randomValue = Random.Range(0f, 100f); //전체 확률 합이 100일때만 유효

        foreach (GachaTable table in tables)
        {
            randomValue -= table.probability;
            if (randomValue <= 0f)
            {
                return table;
            }
        }

        //부동소수점 등 오류 대비
        Debug.LogWarning("비정상적인 결과입니다. common이 출력됩니다.");
        return tables[0];
    }



    //가챠 뽑는 로직
    public int DoGacha()
    {
        //레어도 정하기
        GachaTable table = GetRarity();

        if (table != null)
        {
            int resultId = Random.Range(table.minId, table.maxId + 1);
            Debug.Log($"결과: {table.rarity.ToString()}등급, {resultId}");
            return resultId;
        }

        //예외 처리용
        return -1;
    }
}
