using UnityEngine;

public class AutoRotate : MonoBehaviour
{
    [Header("회전 설정")]
    [Tooltip("초당 회전할 속도입니다. (예: 360 = 1초에 1바퀴)")]
    [SerializeField] private float rotationSpeed = -200f; // (시계 방향으로 돌리려면 음수)


    void Update()
    {
        transform.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime);
    }
}