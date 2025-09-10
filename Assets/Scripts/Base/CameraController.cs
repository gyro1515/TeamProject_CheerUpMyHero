using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] GameObject playerGO;

    private void FixedUpdate()
    {
        if (playerGO == null) return;
        Vector3 camPos = gameObject.transform.position;
        camPos.x = playerGO.transform.position.x;
        gameObject.transform.position = camPos;
    }
}
