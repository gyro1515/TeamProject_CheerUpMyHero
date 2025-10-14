using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIJoyStickVer2 : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    [SerializeField] RectTransform joyStickArea; // 조이스틱 영역
    [SerializeField] GameObject joyStickCenterGO; // 조이스틱 중앙
    [SerializeField] Canvas mainCanvas; // 반지름 보정용 캔버스
    Vector2 centerPos = Vector2.zero;
    float baseRad = 100; // 조이스틱 반지름
    Vector2 dir = Vector2.zero;
    float rad = -1;
    bool isPressing = false;

    private void Awake()
    {
        isPressing = false;
        
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        baseRad = joyStickArea.sizeDelta.x / 2 * 0.8f; // 80프로까지만
        centerPos = joyStickArea.position;

        rad = baseRad * mainCanvas.scaleFactor;
        isPressing = true;
        MovePlayer(eventData);
        //Debug.Log($"조이스틱 반지름: {rad}, 델타 사이즈");
    }
    public void OnDrag(PointerEventData eventData)
    {
        if (!isPressing) return;
        MovePlayer(eventData);
    }
    public void OnPointerUp(PointerEventData eventData)
    {
        isPressing = false;
        joyStickCenterGO.transform.position = centerPos;
        GameManager.Instance.Player.MoveDir = Vector3.zero;
    }
    void MovePlayer(PointerEventData eventData)
    {
        dir = eventData.position - centerPos;
        /*if (Vector2.Dot(dir.normalized, Vector2.right) < 0f)
        {
            Debug.Log($"{dir.normalized}, 좌");
        }
        else Debug.Log($"{dir.normalized}, 우");*/

        dir = Vector2.ClampMagnitude(dir, rad);
        joyStickCenterGO.transform.position = centerPos + dir;
        if (dir.x == 0f) return;
        float playerMoveDir = dir.x < 0f ? -1f : 1f;
        GameManager.Instance.Player.MoveDir = new Vector3(playerMoveDir, 0f, 0f);
    }

    
}
