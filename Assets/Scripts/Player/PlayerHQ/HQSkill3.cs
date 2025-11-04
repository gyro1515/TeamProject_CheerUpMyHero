using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;

public class HQSkill3 : BaseHQSkill
{
    float duration = 1.0f;
    float jumpHeight = 10.0f;
    public override void ActivateSkill(Vector3 start)
    {
        gameObject.transform.position = start;
        if (FindTarget(start, out Vector3 to))
        {
            Debug.Log($"HQ Skill 1 Activated towards {to}");
            //gameObject.transform.DOJump(to, 10f, 1, 1f).onComplete += AttackRange;

            DG.Tweening.Sequence seq = DOTween.Sequence();
            float halfDuration = duration / 2f;
            // 동시에 실행
            // Y축 상승 (0 ~ 0.5초)
            seq.Append(gameObject.transform.DOMoveY(start.y + jumpHeight, halfDuration)
                .SetEase(Ease.OutQuad));
            // Y축 하강 (0.5 ~ 1.0초)
            seq.Append(gameObject.transform.DOMoveY(to.y, halfDuration)
                .SetEase(Ease.InQuad));
            // X축 이동 (전체 시간 동안 Insert)
            seq.Insert(0, gameObject.transform.DOMoveX(to.x, duration)
                .SetEase(Ease.Linear));

            seq.OnComplete(() => AttackRange());
        }
        else
        {
            Debug.Log("HQ Skill 1 Activated but no target found");
            ReleaseSelf();
        }
    }
    protected override void AttackRange()
    {
        // 이펙트
        GameObject fx = ObjectPoolManager.Instance.Get(PoolType.FxHQSkill1Effect);
        Vector3 pos = gameObject.transform.position;
        pos.y += 0.7f;
        fx.transform.position = pos;

        base.AttackRange();
    }
}
