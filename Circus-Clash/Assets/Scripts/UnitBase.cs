using UnityEngine;

public enum UnitTeam { Friendly, Enemy }

[RequireComponent(typeof(Rigidbody2D), typeof(Health))]
public class UnitBase : MonoBehaviour
{
    [Header("Team & Targeting")]
    public UnitTeam team = UnitTeam.Friendly;
    public LayerMask targetMask;     // set via Inspector
    public float detectionRange = 3f;
    public float attackRange = 1.2f;
    public float attackCooldown = 0.8f;

    [Header("Movement")]
    public float moveSpeed = 2.5f;
    public float groundCheckRadius = 0.1f;
    public Transform groundCheck;
    public LayerMask groundMask;

    [Header("Combat")]
    public int damage = 10;

    protected Rigidbody2D rb;
    protected Health myHealth;
    protected Transform currentTarget;
    float lastAttackTime = -999f;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        myHealth = GetComponent<Health>();
    }

    protected virtual void Start()
    {
        // Setup physics defaults
        rb.gravityScale = 3f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    protected virtual void Update()
    {
        if (currentTarget == null || !TargetAlive(currentTarget))
            currentTarget = FindNearestTarget();

        if (currentTarget != null)
        {
            float dist = Vector2.Distance(transform.position, currentTarget.position);
            if (dist > attackRange) MoveToward(currentTarget.position);
            else TryAttack(currentTarget);
        }
        else
        {
            // No target: advance toward enemy base direction
            Vector2 dir = (team == UnitTeam.Friendly) ? Vector2.right : Vector2.left;
            rb.linearvelocity = new Vector2(dir.x * moveSpeed, rb.linearvelocity.y);
        }
    }

    protected virtual void MoveToward(Vector2 pos)
    {
        float dirX = Mathf.Sign(pos.x - transform.position.x);
        rb.linearvelocity = new Vector2(dirX * moveSpeed, rb.linearvelocity.y);
        // Face movement direction
        transform.localScale = new Vector3(Mathf.Sign(dirX), 1, 1);
    }

    protected virtual bool TargetAlive(Transform t)
    {
        if (t == null) return false;
        var h = t.GetComponent<Health>();
        return h != null && h.currentHP > 0;
    }

    protected virtual Transform FindNearestTarget()
    {
        Collider2D[] cols = Physics2D.OverlapCircleAll(transform.position, detectionRange, targetMask);
        Transform best = null;
        float bestDist = Mathf.Infinity;
        foreach (var c in cols)
        {
            float d = Vector2.Distance(transform.position, c.transform.position);
            if (d < bestDist)
            {
                best = c.transform;
                bestDist = d;
            }
        }
        return best;
    }

    protected virtual void TryAttack(Transform t)
    {
        if (Time.time - lastAttackTime < attackCooldown) return;
        lastAttackTime = Time.time;
        DoAttack(t);
    }

    // Melee will override to hit directly; ranged will spawn projectile
    protected virtual void DoAttack(Transform t)
    {
        var h = t.GetComponent<Health>();
        if (h != null) h.TakeDamage(damage);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red; Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.yellow; Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}

