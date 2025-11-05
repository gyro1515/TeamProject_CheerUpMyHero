using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerHQSkill : MonoBehaviour
{
    [SerializeField] List<BaseHQSkill> hQSkills = new List<BaseHQSkill>();
    Vector3 tmpTarget = new Vector3(0f, -100f, 0f);
    Dictionary<int, PoolType> idxToPoolType = new Dictionary<int, PoolType>
    {
        {0, PoolType.HQSkill1 },
        {1, PoolType.HQSkill2 },
        {2, PoolType.HQSkill3 },
        {3, PoolType.HQSkill4 },
        {4, PoolType.HQSkill5 }
    };
    WaitForSeconds waitFindTarget = new WaitForSeconds(0.1f);
    WaitForSeconds waitForAttackDelay = new WaitForSeconds(1f);

    public Dictionary<PoolType, bool> IsCoolTime { get; private set; } = new Dictionary<PoolType, bool>();
    public List<BaseHQSkill> HQSkills { get { return hQSkills; } }
    public List<HQSkillsCooldown> HQSkillsCooldowns { get; private set; } = new List<HQSkillsCooldown>();

    private void Awake()
    {
        IsCoolTime[PoolType.HQSkill1] = false;
        IsCoolTime[PoolType.HQSkill2] = false;
        IsCoolTime[PoolType.HQSkill3] = false;
        IsCoolTime[PoolType.HQSkill4] = false;
        IsCoolTime[PoolType.HQSkill5] = false;
    }
    private void Start()
    {
        StartCoroutine(HQSkillRoutine());
    }
    public PoolType GetPoolTypeByIdx(int idx)
    {
        return idxToPoolType[idx];
    }

    /*private void Update()
    {

        #region 테스트

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            Debug.Log("HQ Skill Activated");
            HQSkill1 hQSkill1 = ObjectPoolManager.Instance.Get(PoolType.HQSkill1).GetComponent<HQSkill1>();
            if (FindTarget(out Vector3 target))
                hQSkill1?.ActivateSkill(gameObject.transform.position, target);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            Debug.Log("HQ Skill Activated");
            HQSkill2 hQSkill1 = ObjectPoolManager.Instance.Get(PoolType.HQSkill2).GetComponent<HQSkill2>();
            if (FindTarget(out Vector3 target))
                hQSkill1?.ActivateSkill(gameObject.transform.position, target);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            Debug.Log("HQ Skill Activated");
            HQSkill3 hQSkill1 = ObjectPoolManager.Instance.Get(PoolType.HQSkill3).GetComponent<HQSkill3>();
            if (FindTarget(out Vector3 target))
                hQSkill1?.ActivateSkill(gameObject.transform.position, target);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            Debug.Log("HQ Skill Activated");
            HQSkill4 hQSkill1 = ObjectPoolManager.Instance.Get(PoolType.HQSkill4).GetComponent<HQSkill4>();
            if (FindTarget(out Vector3 target))
                hQSkill1?.ActivateSkill(gameObject.transform.position, target);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha7))
        {
            Debug.Log("HQ Skill Activated");
            HQSkill5 hQSkill1 = ObjectPoolManager.Instance.Get(PoolType.HQSkill5).GetComponent<HQSkill5>();
            if (FindTarget(out Vector3 target))
                hQSkill1?.ActivateSkill(gameObject.transform.position, target);
        }
        #endregion
    }*/
    IEnumerator HQSkillRoutine()
    {
        //yield return waitFindTarget;
        while(HQSkillsCooldowns.Count == 0)
        {
            yield return null; // UI에서 쿨타임 아이콘 초기화 될 때까지 대기
        }
        while (true)
        {
            if (FindTarget(out Vector3 target))
            {
                for(int i = hQSkills.Count - 1; i >= 0; i--) // 가장 강력한 스킬부터 사용, 쿨타임도 가장 김
                {
                    float dist = target.x - gameObject.transform.position.x;
                    if (dist > hQSkills[i].DetectRange) continue; // 공격 범위 초과하면 다음
                    if (IsCoolTime[idxToPoolType[i]]) continue; // 쿨타임 중이면 다음

                    GameObject hqSkillGO = ObjectPoolManager.Instance.Get(idxToPoolType[i]);
                    BaseHQSkill baseHQSkill = hqSkillGO.GetComponent<BaseHQSkill>();
                    IsCoolTime[idxToPoolType[i]] = true;
                    HQSkillsCooldowns[i].ShowSkillCooldown();
                    baseHQSkill.ActivateSkill(gameObject.transform.position, target);
                    AudioManager.PlayOneShotByCameraDistance(DataManager.AudioData.useHQSkill, gameObject.transform);
                    yield return waitForAttackDelay; // 스킬 사용 후 약간의 딜레이
                }
            }
            yield return waitFindTarget;
        }
    }
    
    protected bool FindTarget(out Vector3 target)
    {
        List<BaseCharacter> enemyList = UnitManager.EnemyUnitList;

        float minDist = float.MaxValue;
        target = tmpTarget;
        foreach (var unit in enemyList)
        {
            if (unit == null || unit.IsDead) continue;

            // 거리 계산
            Vector3 unitPos = unit.gameObject.transform.position;
            //float dist = Mathf.Abs(unitPos.x - callerPos.x);
            float dist = unitPos.x - gameObject.transform.position.x;
            if (dist < 0f) continue; // 반대 방향 공격 x
            if (dist > hQSkills[0].DetectRange) continue; // 공격 범위 초과하면 다음
            if (dist > minDist) continue; // 최소 거리보다 멀다면 다음
            minDist = dist;
            target = unit.gameObject.transform.position;
        }
        if (target != tmpTarget)
        {
            return true;
        }

        return false;
    }
}
