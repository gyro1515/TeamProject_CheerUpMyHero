using System.Collections.Generic;
using UnityEngine;

public abstract class MonoSO<T> : ScriptableObject where T : MonoData
{
    public abstract List<T> GetList(); // 삭제해야 함

    public abstract void SetData(Dictionary<int, T> DB);
}
