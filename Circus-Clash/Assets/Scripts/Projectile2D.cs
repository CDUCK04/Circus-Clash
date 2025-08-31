using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Projectile2D : MonoBehaviour
{
    public float lifeTime = 5f;

    int damage;
    Vector2 velocity;
    string bulletName;

    public void Init(int dmg, Vector2 vel) { damage = dmg; velocity = vel; }

    void Start()
    {
        Destroy(gameObject, lifeTime);
        bulletName = this.name;
    }

    void Update() => transform.Translate(velocity * Time.deltaTime);

    void OnTriggerEnter2D(Collider2D other)
    {
        var hp = other.GetComponentInParent<CircusClash.Troops.Combat.UnitHealth>();

        if (hp == null || hp.IsDead) return;

        if (other.CompareTag(this.tag)) return;

        hp.TakeDamage(damage);
        Destroy(gameObject);
    }
}
