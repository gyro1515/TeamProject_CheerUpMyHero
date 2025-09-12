using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class TestBulletV2 : MonoBehaviour
{
    private BasePoolable poolable;
    private Rigidbody2D rb;
    private void OnEnable()
    {
        poolable = GetComponent<BasePoolable>();
        rb = GetComponent<Rigidbody2D>();
        rb.AddForce(Vector2.right * 50);
        StartCoroutine(ReleaseAfter10s());
    }

    IEnumerator ReleaseAfter10s()
    {
        yield return new WaitForSeconds(10f);
        poolable.ReleaseSelf();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        poolable.ReleaseSelf();
    }
}
