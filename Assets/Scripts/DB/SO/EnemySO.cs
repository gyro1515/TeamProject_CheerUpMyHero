using System.Collections.Generic;

[ExcelAsset(AssetPath = "Resources/DB")]
public class EnemySO : MonoSO<EnemyData>
{
    public List<EnemyData> enemyList = new List<EnemyData>();
    public override List<EnemyData> GetList()
    {
        return enemyList;
    }
}
