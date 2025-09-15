using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneLoaderBattle : MonoBehaviour
{
    [SerializeField] GameObject map;
    private void Awake()
    {
        Instantiate(map);
    }
}

