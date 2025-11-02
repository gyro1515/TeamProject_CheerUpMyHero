using UnityEngine;

public class AttackStateBehaviour : StateMachineBehaviour
{
    private BaseUnit baseUnit;

    // 1. 상태에 "진입"할 때 호출됨
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // 컨트롤러 스크립트 참조 찾기
        if (baseUnit == null) baseUnit = animator.GetComponentInParent<BaseUnit>();
        if (baseUnit)
        {
            baseUnit.IsAttackAnimPlaying = true;
            //Debug.Log("공격 애니메이션 재생 시작");
        }
        else
        {
            Debug.LogError("AttackStateBehaviour: BaseUnit 컴포넌트를 찾을 수 없습니다.");
        }

    }

    // 2. 상태에서 "퇴장"할 때 호출됨 (가장 중요!)
    //    (애니메이션이 끝나거나, 중간에 중단되거나 "무조건" 호출됨)
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // 컨트롤러 스크립트 참조 찾기
        if (baseUnit == null) baseUnit = animator.GetComponentInParent<BaseUnit>();
        if (baseUnit)
        {
            baseUnit.IsAttackAnimPlaying = false;
            //Debug.Log("공격 애니메이션 재생 끝");
        }
        else
        {
            Debug.LogError("AttackStateBehaviour: BaseUnit 컴포넌트를 찾을 수 없습니다.");
        }
    }
}
