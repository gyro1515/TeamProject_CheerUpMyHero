using UnityEngine;

public class ReparentToCanvas : MonoBehaviour
{
    void Awake()
    {
        // 만약 이 오브젝트가 Canvas 밖에 있다면,
        if (transform.GetComponentInParent<Canvas>() == null)
        {
            // 씬에 있는 Canvas를 찾아서
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas != null)
            {
                // 그 Canvas의 자식으로 자신을 옮깁니다.
                transform.SetParent(canvas.transform, false);
            }
            else
            {
                Debug.LogError("씬에 UI를 표시할 Canvas가 없습니다!");
            }
        }
    }
}