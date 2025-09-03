using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Projectile2D : MonoBehaviour
{
    public float lifeTime = 4f;
    Rigidbody2D rb;

    int damage;
    string targetTag;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb) rb.gravityScale = 0f;
        Destroy(gameObject, lifeTime);
    }

    public void Init(int dmg, Vector2 velocity, string targetTag)
    {
        this.damage = dmg;
        this.targetTag = targetTag;
        if (rb) rb.linearVelocity = velocity;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!string.IsNullOrEmpty(targetTag) && !other.CompareTag(targetTag)) return;

        var hp = other.GetComponentInParent<UnitHealth>();
        if (hp != null) hp.TakeDamage(damage);

        Destroy(gameObject);
    }
}