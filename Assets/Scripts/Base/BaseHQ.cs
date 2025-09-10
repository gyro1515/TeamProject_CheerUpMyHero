using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public abstract class BaseHQ : MonoBehaviour
{
    [Header("본부 세팅")]
    [SerializeField] protected float minY = 0;
    [SerializeField] protected float maxY = 0;
    [SerializeField] protected float spawnInterval = 0.5f;
    protected int tmpMinY;
    protected int tmpMaxY;
    protected virtual void Awake()
    {
        tmpMinY = (int)(minY * 100f);
        tmpMaxY = (int)(maxY * 100f) + 1;
        InvokeRepeating("SpawnUnit", 0f, spawnInterval);
    }
    protected abstract void SpawnUnit();
}
