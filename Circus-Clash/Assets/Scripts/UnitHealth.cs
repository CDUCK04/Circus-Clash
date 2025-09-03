using UnityEngine;
using UnityEngine.Events;

public class UnitHealth : MonoBehaviour
{
    public int maxHealth = 100;
    public UnityEvent onDied;
    public UnityEvent<int> onDamaged;
    public UnitBrain unitBrain;

    int current;
    bool deathHandled;                   // NEW: latch
    public bool IsDead => current <= 0;

    void Awake()
    {
        current = maxHealth;
        unitBrain = GetComponent<UnitBrain>();
    }

    public void TakeDamage(int dmg)
    {
        if (IsDead) return;
        current -= Mathf.Max(1, dmg);
        onDamaged?.Invoke(dmg);

        if (current <= 0)
        {
            current = 0;
            if (deathHandled) return;    
            deathHandled = true;

            onDied?.Invoke();

            var rb = GetComponent<Rigidbody2D>();
            if (rb) { rb.linearVelocity = Vector2.zero; rb.angularVelocity = 0f; }

            foreach (var col in GetComponentsInChildren<Collider2D>())
                col.enabled = false;

            if (unitBrain != null)
            {
                unitBrain.SetAnim(false, true);
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}