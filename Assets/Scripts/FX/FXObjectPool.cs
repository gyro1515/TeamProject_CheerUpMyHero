using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FXObjectPool : BasePoolable
{
    [Header("이펙트 지속시간 설정")]
    [SerializeField]float duration = 1.5f;
    [SerializeField] ParticleSystem _particleSystem;

    //private ParticleSystem[] particleSystems;
    WaitForSeconds wait;
    private void Awake()
    {
        wait = new WaitForSeconds(duration);
        //particleSystems = GetComponentsInChildren<ParticleSystem>();
    }
    private void OnEnable()
    {
        /*foreach(var ps in particleSystems)
        {
            ps.Clear(); // 이전 입자 제거
            ps.Play();  // 즉시 재생
        }*/
        if (_particleSystem == null)
        {
            Debug.LogWarning($"{name}의 ParticleSystem이 비어 있습니다!");
            return;
        }
        _particleSystem.Clear(true);
        _particleSystem.Play(true);
        StartCoroutine(ActiveFalseRoutine());
    }
    private void OnDisable()
    {
        ReleaseSelf();
    }
    IEnumerator ActiveFalseRoutine()
    {
        yield return wait;
        if(gameObject.activeSelf) gameObject.SetActive(false);
    }
}
