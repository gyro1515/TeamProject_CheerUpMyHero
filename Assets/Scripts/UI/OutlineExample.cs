using TMPro;
using UnityEngine;

public class OutlineExample : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI tmpText;

    void Start()
    {
        tmpText.outlineColor = Color.black;

        // 외곽선 두께 (0 ~ 1 사이 값)
        tmpText.outlineWidth = 0.1f;
    }
}
