using System.Collections.Generic;
using UnityEngine;

public abstract class MonoSO<T> : ScriptableObject where T : MonoData
{
    public abstract List<T> GetList();
}
