using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneLoaderBattle : MonoBehaviour
{
    [SerializeField] GameObject map;
    private void Awake()
    {
        UIManager.Instance.GetUI<UITest>();
        Instantiate(map);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SceneLoader.Instance.StartLoadScene(SceneState.MainScene);
        }
    }
}

