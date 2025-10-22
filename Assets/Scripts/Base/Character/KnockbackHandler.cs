using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using DG.Tweening;

public class KnockbackHandler : MonoBehaviour
{
    [Header("히트백/넉백 설정")]
    [SerializeField] float hitBackTime = 1.0f;
    [SerializeField] float hitBackScale = 3.0f;
    [SerializeField] float knockBackTime = 0.2f;
    [SerializeField] float knockBackScale = 0.3f;
    [SerializeField] float yPower = 2f;
    [SerializeField] Ease easeType = Ease.OutQuad;

    //float hitBackTimer = 0.0f;
    [SerializeField] float hitBackDistance = 0.0f;
    //float knockBackTimer = 0.0f;
    [SerializeField] float knockBackDistance = 0.0f;

    public event Action<bool> OnHitBackActive;

    BaseUnit baseUnit;
    float hitBackDir = 0;
    /*private void Awake()
    {
        if (hitBackDir == 0f)
        {
            //Debug.LogWarning("KnockbackHandler Awake()안됨");
            baseUnit = GetComponent<BaseUnit>();
            baseUnit.OnHitBack += ApplyHitBack;
            baseUnit.OnKnockBack += ApplyKnockBack;
            // 추후 리팩토링 필요 **************
            hitBackDir = baseUnit is EnemyUnit ? 1f : -1f;
        }
    }*/
    // 유닛 활성화때마다 초기화 과정 필수, 유닛 크기, 체력이 달라질 수 있기 때문
    public void Init(float unitSize)
    {
        if (hitBackDir == 0f)
        {
            if (baseUnit == null)
            {
                baseUnit = GetComponent<BaseUnit>();
                baseUnit.OnHitBack += ApplyHitBack;
                baseUnit.OnKnockBack += ApplyKnockBack;
            }
            hitBackDir = baseUnit is EnemyUnit ? 1f : -1f;
        }
        hitBackDistance = unitSize * hitBackScale * hitBackDir;
        knockBackDistance = unitSize * knockBackScale * hitBackDir;
        if(hitBackDistance == 0f || knockBackDistance == 0f)
        {
            Debug.Log("왜 0이야.");
        }
    }
    void ApplyHitBack()
    {
        // 히트백 시작
        //Debug.Log("히트백 시작!!!");
        OnHitBackActive?.Invoke(true);
        StartCoroutine(HitBackRoutine());
    }
    public void ApplyKnockBack()
    {
        //Debug.Log("넉백!!!");
        //StartCoroutine(KnockBackRoutine());
        if (baseUnit == null || baseUnit.gameObject == null) return;
        float startPosX = baseUnit.gameObject.transform.position.x;
        float endPosX = startPosX + knockBackDistance;
        baseUnit?.gameObject?.transform.DOMoveX(endPosX, knockBackTime).SetEase(Ease.OutQuad);
    }
    IEnumerator HitBackRoutine()
    {
        if (baseUnit == null || baseUnit.gameObject == null) { OnHitBackActive?.Invoke(false); yield break; } // 히트백/코루틴 중지

        //hitBackTimer = 0f;
        float getUpTime = 0.4f; // 애니메이션 일어나는 시간
        float startPosX = baseUnit.gameObject.transform.position.x;
        float endPosX = startPosX + hitBackDistance;

        // 두트윈으로?
        float startPosY = baseUnit.gameObject.transform.position.y;
        float endPosY = startPosY + yPower;
        baseUnit.gameObject.transform.DOMoveX(endPosX, hitBackTime).SetEase(easeType);
        baseUnit.gameObject.transform.DOMoveY(endPosY, hitBackTime / 2).SetEase(easeType);
        yield return new WaitForSeconds(hitBackTime / 2);
        if (baseUnit == null || baseUnit.gameObject == null) { OnHitBackActive?.Invoke(false); yield break; } // 히트백/코루틴 중지

        baseUnit.gameObject.transform.DOMoveY(startPosY, hitBackTime / 2).SetEase(Ease.OutBounce);
        yield return new WaitForSeconds(hitBackTime / 2);

        if(baseUnit == null || baseUnit.gameObject == null || baseUnit.BaseController == null || baseUnit.BaseController.Animator == null)
        {
            Debug.LogError("히트백 도중 유닛 또는 애니메이터 소멸. Why????");
            OnHitBackActive?.Invoke(false); // 히트백 끝
            yield break;
        }
        // 일어서야 할 때 캐릭터가 죽은 상태라면
        if (baseUnit.IsDead)
        {
            //Debug.Log("죽음");
            OnHitBackActive?.Invoke(false);
            //if (baseUnit.TryGetComponent<Player>(out Player player))
            if (baseUnit is Player player)
                Debug.Log("플레이어 넉백 후 사망");
            else
                baseUnit.UnitController.SetDead();
            yield break;
        }
        // 죽지 않았다면
        //if(baseUnit && baseUnit.BaseController && baseUnit.BaseController.Animator) 
            baseUnit.BaseController.Animator.SetBool(baseUnit.AnimationData.GetUpParameterHash, true);
        yield return new WaitForSeconds(getUpTime);
        //if (baseUnit && baseUnit.BaseController && baseUnit.BaseController.Animator)
            baseUnit.BaseController.Animator.SetBool(baseUnit.AnimationData.GetUpParameterHash, false);
        OnHitBackActive?.Invoke(false); // 히트백 끝
        //Debug.Log("히트백 끝!!!");

    }

    /*IEnumerator KnockBackRoutine()
    {
        knockBackTimer = 0f;
        float startPosX = baseUnit.gameObject.transform.position.x;
        float endPosX = startPosX + hitBackDir * knockBackDistance;
        float targetPosX = 0f;
        Vector3 nextPos;

        while (knockBackTimer < knockBackTime)
        {
            knockBackTimer += Time.fixedDeltaTime;
            targetPosX = Mathf.Lerp(startPosX, endPosX, knockBackTimer / knockBackTime);
            nextPos = baseUnit.gameObject.transform.position;
            nextPos.x = targetPosX;
            baseUnit.gameObject.transform.position = nextPos;
            yield return new WaitForFixedUpdate();
        }
    }*/

}
