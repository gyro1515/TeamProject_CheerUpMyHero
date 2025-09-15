using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trigger : MonoBehaviour
{
    public SceneState startScene;

    void Awake()
    {
        SceneLoadManager.Instance.LoadScene(startScene);
    }
}
