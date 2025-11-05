using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 이 오브젝트가 활성화될 때마다,
/// 연결된 배경 목록 중 하나만 랜덤으로 골라 켭니다.
/// </summary>
public class MenuBackgroundChanger : MonoBehaviour
{
    [Header("배경 오브젝트 목록")]
    [SerializeField] private List<GameObject> backgroundObjects;

    private GameObject _currentBackground = null;

    private void OnEnable()
    {
        SetRandomBackground();
    }

    private void SetRandomBackground()
    {
        if (backgroundObjects == null || backgroundObjects.Count == 0)
        {
            Debug.LogWarning("배경 이미지 목록이 비어있습니다.");
            return;
        }

        // 1. 현재 켜져있는 배경(이전에 켰던)을 끕니다.
        if (_currentBackground != null)
        {
            _currentBackground.SetActive(false);
        }

        // 2. 목록에서 랜덤으로 하나를 뽑습니다.
        int randomIndex = Random.Range(0, backgroundObjects.Count);
        _currentBackground = backgroundObjects[randomIndex];

        // 3. 뽑힌 배경을 켭니다.
        if (_currentBackground != null)
        {
            _currentBackground.SetActive(true);
        }
    }
}