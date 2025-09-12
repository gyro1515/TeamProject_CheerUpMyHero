using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneLoaderBattle : MonoBehaviour
{
    [SerializeField] GameObject map;
    [SerializeField] GameObject player;
    private void Awake()
    {
        Instantiate(map);
        //Instantiate(player);
    }
}

