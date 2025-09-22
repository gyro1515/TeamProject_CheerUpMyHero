using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIMoveButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [Header("UI 버튼 설정")]
    [SerializeField] float dir = 0f; // 인스펙터 창에서 설정
    public void OnPointerDown(PointerEventData eventData)
    {
        GameManager.Instance.Player.MoveDir = new Vector3(dir, 0f, 0f);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        GameManager.Instance.Player.MoveDir = Vector3.zero;
    }
}
