using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneLoaderStart : MonoBehaviour
{
    public SceneState nextScene;

    async void Awake()
    {
        //백엔드 매니저 말고도, 파괴되지 않는 매니저는 넣어도 됨
        
        await BackendManager.EnsureInstanceAndInitializedAsync();

        UIManager.Instance.OpenUI<StartUI>();
    }
}
