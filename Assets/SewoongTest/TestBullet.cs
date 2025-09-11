using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestBullet : BasePoolable
{
    private Rigidbody2D rb;
    private void OnEnable()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.AddForce(Vector2.right * 50);
        StartCoroutine(ReleaseAfter10s());
    }

    IEnumerator ReleaseAfter10s()
    {
        yield return new WaitForSeconds(10f);
        ReleaseSelf();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        ReleaseSelf();
    }


}
