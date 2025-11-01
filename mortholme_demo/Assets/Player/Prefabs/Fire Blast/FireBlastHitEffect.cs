using UnityEngine;

public class FireBlastHitEffect : MonoBehaviour
{
    public float knockback = 12f;

    private void Awake()
    {
        Destroy(this.gameObject, 3f);
    }

    public void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.tag == "Hero")
        {
            // - Deal Damage
            col.GetComponent<HealthScript>().DealDamage(7f);

            // - Launch Target Back
            Rigidbody2D rigid = col.gameObject.GetComponent<Rigidbody2D>();
            Vector3 launchDir = (col.gameObject.transform.position - transform.position).normalized;
            launchDir += Vector3.up * 0.4f;
            float launchForce = knockback;
            rigid.AddForce(launchDir * launchForce, ForceMode2D.Impulse);
        }
    }
}
