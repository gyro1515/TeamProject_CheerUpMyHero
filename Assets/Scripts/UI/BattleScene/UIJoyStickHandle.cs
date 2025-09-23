using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIJoyStickHandle : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerMoveHandler
{
    //[Header("조이스틱 설정")]
    float baseRad = 100; // 조이스틱 반지름
    Vector2 keyDowmPos = Vector2.zero;
    Vector2 dir = Vector2.zero;
    float rad = -1;
    bool isPressing = false;
    UIJoyStick joyStick;
    private void Awake()
    {
        // 캔버스 따로 만들어서 맨 위에 그려지게 하기
        joyStick = UIManager.Instance.GetUI<UIJoyStick>();
        joyStick.JoyStickGO.SetActive(false);
        isPressing = false;
        baseRad = joyStick.JoyStickGO.GetComponent<RectTransform>().sizeDelta.x / 2 * 0.8f; // 80프로까지만
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        joyStick.JoyStickGO.SetActive(true);
        joyStick.JoyStickGO.transform.position = eventData.position;
        joyStick.JoyStickCenterGO.transform.position = eventData.position;
        keyDowmPos = eventData.position;
        rad = baseRad * joyStick.MainCanvas.scaleFactor;
        isPressing = true;
    }

    public void OnPointerMove(PointerEventData eventData)
    {
        if (!isPressing) return;
        dir = eventData.position - keyDowmPos;
        /*if (Vector2.Dot(dir.normalized, Vector2.right) < 0f)
        {
            Debug.Log($"{dir.normalized}, 좌");
        }
        else Debug.Log($"{dir.normalized}, 우");*/

        dir = Vector2.ClampMagnitude(dir, rad);
        joyStick.JoyStickCenterGO.transform.position = keyDowmPos + dir;
        if (dir.x == 0f) return;
        float playerMoveDir = dir.x < 0f ? -1f : 1f;
        GameManager.Instance.Player.MoveDir = new Vector3(playerMoveDir, 0f, 0f);
        //Debug.Log($"{dir.magnitude} / {rad}");
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isPressing = false;
        joyStick.JoyStickGO.SetActive(false);
        GameManager.Instance.Player.MoveDir = Vector3.zero;
    }
}
