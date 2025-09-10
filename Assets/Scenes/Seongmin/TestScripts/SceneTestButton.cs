using UnityEngine;

public class SceneTestButton : MonoBehaviour
{
    public SceneState nextSceneState;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            SceneLoadManager.Instance.LoadScene(nextSceneState);
        }
    }
}