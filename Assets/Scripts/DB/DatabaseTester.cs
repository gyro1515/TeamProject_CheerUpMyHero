using UnityEngine;
using System.Linq;

public class DatabaseTester : MonoBehaviour
{
    [Header("테스트 설정")]
    [SerializeField] private bool testOnStart = true;
    [SerializeField] private bool showDetailedInfo = true;

    void Start()
    {
        if (testOnStart)
        {
            TestAllDatabases();
        }
    }

    [ContextMenu("데이터베이스 테스트 실행")]
    public void TestAllDatabases()
    {
        Debug.Log("=== 데이터베이스 테스트 시작 ===");

        TestItemDatabase();
        TestEnemyDatabase();

        Debug.Log("=== 데이터베이스 테스트 완료 ===");
    }

    void TestItemDatabase()
    {
        Debug.Log("\n--- 아이템 데이터베이스 테스트 ---");

        var itemDB = DataManager.Instance.ItemData;

        // 기본 정보 출력
        Debug.Log($"아이템 DB 총 개수: {itemDB.Count}");

        // 개별 아이템 테스트 (1, 2, 3번)
        for (int i = 1; i <= 3; i++)
        {
            var item = itemDB.GetData(i);
            if (item != null)
            {
                Debug.Log($"✅ 아이템 {i}번 발견: {item.name} (가격: {item.price}, 설명: {item.description})");
            }
            else
            {
                Debug.LogWarning($"❌ 아이템 {i}번을 찾을 수 없습니다!");
            }
        }

        // 존재하지 않는 아이템 테스트
        var nonExistentItem = itemDB.GetData(999);
        if (nonExistentItem == null)
        {
            Debug.Log("✅ 존재하지 않는 아이템(999번) 처리 정상");
        }

        // Keys와 Values 테스트
        if (showDetailedInfo)
        {
            Debug.Log($"모든 아이템 ID: [{string.Join(", ", itemDB.Keys)}]");
            Debug.Log("모든 아이템 이름: [" + string.Join(", ", itemDB.Values.Select(item => item.name)) + "]");
        }
    }

    void TestEnemyDatabase()
    {
        Debug.Log("\n--- 적 데이터베이스 테스트 ---");

        var enemyDB = DataManager.Instance.EnemyData;

        // 기본 정보 출력
        Debug.Log($"적 DB 총 개수: {enemyDB.Count}");

        // 개별 적 테스트 (101, 102, 103번)
        for (int i = 101; i <= 103; i++)
        {
            var enemy = enemyDB.GetData(i);
            if (enemy != null)
            {
                Debug.Log($"✅ 적 {i}번 발견: {enemy.name} (체력: {enemy.hp}, 속도: {enemy.speed})");
            }
            else
            {
                Debug.LogWarning($"❌ 적 {i}번을 찾을 수 없습니다!");
            }
        }

        // 존재하지 않는 적 테스트
        var nonExistentEnemy = enemyDB.GetData(999);
        if (nonExistentEnemy == null)
        {
            Debug.Log("✅ 존재하지 않는 적(999번) 처리 정상");
        }

        // Keys와 Values 테스트
        if (showDetailedInfo)
        {
            Debug.Log($"모든 적 ID: [{string.Join(", ", enemyDB.Keys)}]");
            Debug.Log("모든 적 이름: [" + string.Join(", ", enemyDB.Values.Select(enemy => enemy.name)) + "]");
        }
    }

    [ContextMenu("개별 데이터 조회 테스트")]
    public void TestIndividualData()
    {
        Debug.Log("\n=== 개별 데이터 조회 테스트 ===");

        // 특정 아이템 조회
        TestSpecificItem(1);
        TestSpecificItem(2);
        TestSpecificEnemy(101);
        TestSpecificEnemy(102);
    }

    void TestSpecificItem(int id)
    {
        var item = DataManager.Instance.ItemData.GetData(id);
        if (item != null)
        {
            Debug.Log($"아이템 {id}: {item.name} - {item.description} (₩{item.price})");
        }
        else
        {
            Debug.LogWarning($"아이템 {id}를 찾을 수 없음");
        }
    }

    void TestSpecificEnemy(int id)
    {
        var enemy = DataManager.Instance.EnemyData.GetData(id);
        if (enemy != null)
        {
            Debug.Log($"적 {id}: {enemy.name} - HP:{enemy.hp} SPD:{enemy.speed}");
        }
        else
        {
            Debug.LogWarning($"적 {id}를 찾을 수 없음");
        }
    }

    [ContextMenu("성능 테스트")]
    public void PerformanceTest()
    {
        Debug.Log("\n=== 성능 테스트 ===");

        var watch = System.Diagnostics.Stopwatch.StartNew();

        // 1000번 조회 테스트
        for (int i = 0; i < 1000; i++)
        {
            DataManager.Instance.ItemData.GetData(1);
            DataManager.Instance.ItemData.GetData(101);
        }

        watch.Stop();
        Debug.Log($"1000번 조회 시간: {watch.ElapsedMilliseconds}ms");
    }
}
