using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataBase<T1, T2> where T1 : MonoData where T2 : MonoSO<T1>
{
    private readonly Dictionary<int, T1> _DB;       // Dictionary 형태로 데이터 캐싱함.
    public int Count => _DB.Count;                  // Count : DB 내 데이터 수 세어줌. 튜플이 몇 개인지
    public IEnumerable<int> Keys => _DB.Keys;       // Keys : IEnumable로 딕셔너리 key들 순회할 수 있게 해줌 -> 요걸로 foreach문 사용 가능함
    public IEnumerable<T1> Values => _DB.Values;    // Values : IEnumable로 딕셔너리 value들 순회할 수 있게 해줌 -> foreach

    public T2 SO {  get; private set; }             // 데이터 SO 파일 저장해주는 전역변수 -> 다른 곳에서도 사용할 수 있음

    public DataBase(string fileName = null)
    {
        // SO 파일 저장할 경로 설정
        string realFileName = fileName ?? typeof(T2).Name;  // 매개변수로 따로 지정 안 해주면 T2(SO) 파일 이름 넣어줌.
        string filePath = $"DB/{realFileName}";

        SO = Resources.Load<T2>(path: filePath);            // so : 데이터 SO 파일. 지정된 경로에서 SO를 불러와서 so변수에 저장해줌
        
        if (SO  == null)
        {
            Debug.Log("SO 파일 없어요 데이터 임포트 관련 뭔가 문제 있음");
            return;
        }
        _DB = new Dictionary<int, T1>();

        SO.SetData(_DB);

        /*List<T1> list = SO.GetList();                       // list : SO에 저장된 엑셀 데이터. SO에 정의된 엑셀 데이터를 할당해둔 리스트 가져옴

        if (list == null || list.Count == 0)                // 파일을 못 불러오거나 파일에 담긴 정보가 없을 때
        {
            _DB = new Dictionary<int, T1>();                // 걍 딕셔너리 초기화하고 메서드 종료함.
            return;
        }

        _DB = new Dictionary<int, T1>(list.Count);          // item 정보를 넣을 딱 필요한 크기(list.Count) 만큼의 딕셔너리를 생성함.
        for (int i = 0; i < list.Count; i++)                // 엑셀 데이터 개수만큼 반복문 반복, 데이터 가져와서 딕셔너리에 넣어주는 반복문
        {
            var data = list[i];                             // data : 엑셀 데이터 중 i번째 데이터.
            if (data == null) continue;

            if (_DB.ContainsKey(data.idNumber))             // 데이터에서 i번째 데이터의 id 숫자 있는지 검색하고 있으면 에러 메세지
                Debug.LogWarning($"ID 중복 있음 {data.idNumber} ID 중복임");

            _DB[data.idNumber] = data;                      // 딕셔너리에 key 값을 idNumber로, value 값을 엑셀의 i번째 데이터로 저장함.
        }*/
    }

    public T1 GetData(int idNumber)                 // 데이터 얻어오는 함수
    {
        _DB.TryGetValue(idNumber, out var data);    // idNumber를 키 값으로 하는 value를 data에 넣어줌. 해당하는 키 값 없으면 false 나옴 
        return data;
    }

    //데이터의 개수를 얻어오는 함수가 있으면 좋을 것 같아서 추가했습니다.
    public int GetDataSize()
    {
        return _DB.Count;
    }

    public bool TryGetValue(int idNumber, out T1 data)      // 각 데이터에 TryGetValue 못 쓰는 거 개빡쳐서 만들었어용
    {
        return _DB.TryGetValue(idNumber, out data);
    }
}
