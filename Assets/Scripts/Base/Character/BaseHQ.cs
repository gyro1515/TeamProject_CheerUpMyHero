using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;

public abstract class BaseHQ : BaseCharacter, IDamageable
{
    public struct SpawnHQEvent
    {
        public BaseHQ baseHQ;
        public EUIHpBarType type;
        public Vector2? hpBarSize;
        public bool isPlayer;
    }
    [Header("본부 세팅")]
    [SerializeField] protected float minY = 0; // 스폰 위치 최소값
    [SerializeField] protected float maxY = 0; // 스폰 위치 최대값
    [SerializeField] protected float spawnInterval = 0.2f;
    int tmpMinY;
    int tmpMaxY;

    protected override void Awake()
    {
        base.Awake();
        curHp = MaxHp;
        tmpMinY = (int)(minY * 100f);
        tmpMaxY = (int)(maxY * 100f) + 1;
        OnDead += Dead;
    }
    //protected abstract void SpawnUnit(); // 추후 애너미에만 있을 지도...?
    public virtual void Dead()
    {
        // 여기서 오브젝트 풀 반환
        CancelInvoke("SpawnUnit"); //게임 매니저에 있는 Time.timeScale = 0f;일시정지일뿐이라서 시간이 다시 흐르면 멈췄던 Invoke가 재시작되므로,
                                   //'완전한 종료'를 위해 CancelInvoke가 필요
        OnDead -= Dead;
        AudioManager.PlayOneShotByCameraDistance(DataManager.AudioData.hqDestroySE, gameObject.transform);
        Debug.Log("HQDead");
        gameObject.SetActive(false);
        Destroy(gameObject);
    }
    public void TakeDamage(float damage)
    {
        CurHp -= damage;
    }
    public Vector3 GetRandomSpawnPos()
    {
        Vector3 spawnPos = gameObject.transform.position;
        //spawnPos.x += UnityEngine.Random.Range(-0.5f, 0.5f); // 테스트용 코드, 한번에 소환할 떄 x축 약간 랜덤하게 퍼지도록
        spawnPos.y += UnityEngine.Random.Range(tmpMinY, tmpMaxY) / 100f;
        return spawnPos;
    }

    bool IDamageable.IsDead()
    {
        return IsDead;
    }

    public void TakeHeal(float amount)
    {
        Debug.LogError("로직 오류 HQ는 힐 대상 아님");
    }
}
