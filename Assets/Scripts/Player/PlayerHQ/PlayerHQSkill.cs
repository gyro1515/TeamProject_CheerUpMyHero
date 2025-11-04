using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHQSkill : MonoBehaviour
{
    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Alpha3))
        {
            Debug.Log("HQ Skill Activated");
            HQSkill1 hQSkill1 = ObjectPoolManager.Instance.Get(PoolType.HQSkill1).GetComponent<HQSkill1>();
            hQSkill1?.ActivateSkill(gameObject.transform.position);
        }
    }
}
