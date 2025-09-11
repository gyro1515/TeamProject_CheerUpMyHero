using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestTurret : MonoBehaviour
{
    private WaitForSeconds waitHalfSecond;

    private void Start()
    {
        waitHalfSecond = new WaitForSeconds(0.5f);
        StartCoroutine(FireRepeat());
    }

    IEnumerator FireRepeat()
    {
        while (true)
        {
            GameObject obj = ObjectPoolManager.Instance.Get(PoolType.TestBullet);
            obj.transform.position = this.transform.position;
            yield return waitHalfSecond;
        }
    }
}
