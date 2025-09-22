using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class UISafeArea : MonoBehaviour
{
    private RectTransform panel;
    // 아래 두 개는 나중에는 안 쓸듯...?
    private Rect lastSafeArea = Rect.zero; 
    private Vector2Int lastScreenSize = Vector2Int.zero;

    void Awake()
    {
        panel = GetComponent<RectTransform>();
    }
    private void OnEnable()
    {
        ApplySafeArea();
    }
    void Update()
    {
        // SafeArea 변경 or 화면 크기 변경 → 다시 적용
        if (lastSafeArea != Screen.safeArea ||
            lastScreenSize.x != Screen.width ||
            lastScreenSize.y != Screen.height)
        {
            ApplySafeArea();
        }
    }
    void ApplySafeArea()
    {
        Rect safeArea = Screen.safeArea;
        lastSafeArea = safeArea;
        lastScreenSize = new Vector2Int(Screen.width, Screen.height);

        Vector2 anchorMin = safeArea.position;
        Vector2 anchorMax = safeArea.position + safeArea.size;

        // SafeArea를 0~1 anchor 좌표계로 변환
        anchorMin.x /= Screen.width;
        anchorMin.y /= Screen.height;
        anchorMax.x /= Screen.width;
        anchorMax.y /= Screen.height;

        panel.anchorMin = anchorMin;
        panel.anchorMax = anchorMax;

        //Debug.Log($"[SafeAreaHandler] {gameObject.name} 적용됨 → {anchorMin} ~ {anchorMax}");
    }
}
