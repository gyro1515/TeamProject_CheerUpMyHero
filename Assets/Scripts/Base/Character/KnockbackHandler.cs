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
    //float hitBackTimer = 0.0f;
    float hitBackDistance = 0.0f;
    //float knockBackTimer = 0.0f;
    float knockBackDistance = 0.0f;

    public event Action<bool> OnHitBackActive;

    BaseUnit baseUnit;
    float hitBackDir = 0;
    private void Awake()
    {
        /*baseUnit = GetComponent<BaseUnit>();
        baseUnit.OnHitBack += ApplyHitBack;
        baseUnit.OnKnockBack += ApplyKnockBack;
        // 추후 리팩토링 필요 **************
        hitBackDir = baseUnit is EnemyUnit ? 1f : -1f;*/
    }
    // 유닛 활성화때마다 초기화 과정 필수, 유닛 크기, 체력이 달라질 수 있기 때문
    public void Init(float unitSize)
    {
        if(hitBackDir == 0f)
        {
            //Debug.LogWarning("KnockbackHandler Awake()안됨");
            baseUnit = GetComponent<BaseUnit>();
            baseUnit.OnHitBack += ApplyHitBack;
            baseUnit.OnKnockBack += ApplyKnockBack;
            // 추후 리팩토링 필요 **************
            hitBackDir = baseUnit is EnemyUnit ? 1f : -1f;
        }
        hitBackDistance = unitSize * hitBackScale * hitBackDir;
        knockBackDistance = unitSize * knockBackScale * hitBackDir;
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
        float startPosX = baseUnit.gameObject.transform.position.x;
        float endPosX = startPosX + knockBackDistance;
        baseUnit.gameObject.transform.DOMoveX(endPosX, knockBackTime).SetEase(Ease.OutQuad);
    }
    IEnumerator HitBackRoutine()
    {
        //hitBackTimer = 0f;
        float getUpTime = 0.4f; // 애니메이션 일어나는 시간
        float startPosX = baseUnit.gameObject.transform.position.x;
        float endPosX = startPosX + hitBackDistance;

        // 두트윈으로?
        float startPosY = baseUnit.gameObject.transform.position.y;
        float endPosY = startPosY + yPower;
        baseUnit.gameObject.transform.DOMoveX(endPosX, hitBackTime).SetEase(Ease.OutQuad);
        baseUnit.gameObject.transform.DOMoveY(endPosY, hitBackTime / 2).SetEase(Ease.OutQuad);
        yield return new WaitForSeconds(hitBackTime / 2);
        baseUnit.gameObject.transform.DOMoveY(startPosY, hitBackTime / 2).SetEase(Ease.OutBounce);
        yield return new WaitForSeconds(hitBackTime / 2);

        // 수동으로?
        /*while (hitBackTimer < hitBackTime) // 날아가는 시간
        {
            hitBackTimer += Time.fixedDeltaTime;
            targetPosX = Mathf.Lerp(startPosX, endPosX, hitBackTimer / hitBackTime);
            nextPos = baseUnit.gameObject.transform.position;
            nextPos.x = targetPosX;
            baseUnit.gameObject.transform.position = nextPos;
            yield return new WaitForFixedUpdate();
        }*/
        // 일어서야 할 때 캐릭터가 죽은 상태라면
        if (baseUnit && baseUnit.IsDead)
        {
            Debug.Log("죽음");
            OnHitBackActive?.Invoke(false);
            baseUnit.UnitController.SetDead();
            yield break;
        }
        // 죽지 않았다면
        baseUnit.BaseController.Animator.SetBool("isGetUp", true);
        yield return new WaitForSeconds(getUpTime);
        baseUnit.BaseController.Animator.SetBool("isGetUp", false);
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
