using System.Linq;
using UnityEngine;

[RequireComponent(typeof(UnitHealth))]
public class UnitBrain : MonoBehaviour
{
    [Header("Team")]
    public bool isPlayerUnit = true;

    [Header("Motion")]
    public float moveSpeed = 1.8f;
    public bool startStopped = true;
    public float stopDistance = 0.05f;

    [Header("Sensing")]
    public float sightRange = 5f;
    public LayerMask unitMask;
    public bool onlyInFront = true;

    [Header("Attack")]
    public bool isRanged = false;
    public int attackDamage = 10;
    public float attackRange = 1.4f;
    public float attackRate = 1.0f; 
    public Projectile2D projectilePrefab;
    public Transform muzzle;
    public float projectileSpeed = 8f;

    [Header("Animation (optional)")]
    public Animator anim;
    public string isMovingParam = "IsMoving";
    public string isDeadParam = "IsDead";
    public string attackTrigger = "Attack";

    [Header("Targeting")]
    public bool pursueTargetInSight = true;   

    [Header("Separation (anti-overlap)")]
    public bool enableSeparation = true;     
    public float separationRadius = 0.5f;     
    public float separationForce = 2f;       
    public int separationMaxNeighbors = 6;   
    public LayerMask allyMask;                

    // runtime
    UnitHealth hp;
    Transform enemyTent;
    Transform playerTent;
    Transform foeTent;
    FormationManager formation;
    Vector3 formationPos;
    bool hasFormationPos;
    float nextAttackTime;
    Transform target;

    Vector3 homeSlot;        
    bool homeSlotSet;

    Vector3 holdPoint;       
    bool holdingHere;

    bool armed;       
    Transform attackTarget;

    bool armedMelee;
    Transform meleeTarget;

    ArmyStance _stance;
    void Awake()
    {
        hp = GetComponent<UnitHealth>();
        var dir = BattleDirector.Instance;
        if (dir)
        {
            foeTent = isPlayerUnit ? dir.enemyTent : dir.playerTent;

            if (isPlayerUnit)
                dir.OnStanceChanged += OnStanceChanged;
            else if (EnemyCommander.Instance)
                EnemyCommander.Instance.OnStanceChanged += OnStanceChanged;
        }
        formation = FindObjectOfType<FormationManager>();
    }

    void Start()
    {
        if (startStopped) _stopped = true;

        if (isPlayerUnit)
        {
            var bd = BattleDirector.Instance;
            ApplyStance(bd ? bd.CurrentStance : ArmyStance.Retreat);
        }
        else
        {
            var ed = EnemyCommander.Instance;
            ApplyStance(ed ? ed.CurrentStance : ArmyStance.Attack);
        }
    }

    void OnDestroy()
    {
        if (isPlayerUnit && BattleDirector.Instance)
            BattleDirector.Instance.OnStanceChanged -= OnStanceChanged;
        if (!isPlayerUnit && EnemyCommander.Instance)
            EnemyCommander.Instance.OnStanceChanged -= OnStanceChanged;

        if (formation && isPlayerUnit) formation.Unregister(this);
    }

    public void SetFormationSlot(Vector3 worldPos, int row, int col)
    {
        homeSlot = worldPos;
        homeSlotSet = true;
        
    }

    void OnStanceChanged(ArmyStance s)
    {
        if (!isPlayerUnit) return; 
        ApplyStance(s);
    }

    bool _stopped;
    Vector2 MoveDir => isPlayerUnit ? Vector2.right : Vector2.left;

    void ApplyStance(ArmyStance s)
    {
        _stance = s;

        switch (s)
        {
            case ArmyStance.Defend:
                holdingHere = true;
                holdPoint = transform.position;
                _stopped = true;
                break;

            case ArmyStance.Attack:
                holdingHere = false;
                _stopped = false;
                break;

            case ArmyStance.Retreat:
                holdingHere = false;
                _stopped = false;
                break;
        }
    }

    void Update()
    {
        if (hp.IsDead) { SetAnim(false, true); return; }

        // Acquire target and range check
        target = FindClosestEnemy();
        bool inRange = target && InAttackRange(target);

        // Current stance
        var stance = _stance;

        // Decide destination per stance
        Vector3 dest = transform.position;

        // If we SEE a target and are NOT in range, PURSUE it (fixes “walk past”)
        if (pursueTargetInSight && target && !inRange)
        {
            dest = target.position; // go to target’s x & y until in range
        }
        else
        {
            if (stance == ArmyStance.Attack)
            {
                float yLine = homeSlotSet ? homeSlot.y : transform.position.y;
                float xGoal = (isPlayerUnit ? (BattleDirector.Instance ? BattleDirector.Instance.enemyTent : null)
                                            : (BattleDirector.Instance ? BattleDirector.Instance.playerTent : null))
                                            ? (isPlayerUnit ? BattleDirector.Instance.enemyTent.position.x
                                                            : BattleDirector.Instance.playerTent.position.x)
                                            : transform.position.x;
                dest = new Vector3(xGoal, yLine, transform.position.z);
            }
            else if (stance == ArmyStance.Retreat)
            {
                dest = homeSlotSet ? homeSlot : transform.position;
            }
            else /* Defend */
            {
                dest = holdingHere ? holdPoint : transform.position;
            }
        }

        // Act
        if (inRange)
        {
            _stopped = true;
            TryAttack(target);
        }
        else
        {
            if (stance == ArmyStance.Defend && (!target || !pursueTargetInSight))
            {
                // Hold exactly where Defend was pressed (unless we’re actively pursuing a seen target)
                _stopped = true;
            }
            else
            {
                _stopped = false;
                MoveToward(dest); // <-- separation happens inside MoveToward now
            }
        }

        // Face target or travel direction
        Face((target ? (target.position.x - transform.position.x) : (isPlayerUnit ? +1f : -1f)) >= 0f);

        // Anim flags
        SetAnim(!_stopped, false);
    }

    void MoveToward(Vector3 dest)
    {
        Vector3 to = dest - transform.position;
        if (to.sqrMagnitude < stopDistance * stopDistance) { _stopped = true; return; }

        Vector2 dir = new Vector2(to.x, to.y).normalized;

        if (enableSeparation)
        {
            Vector2 sep = ComputeSeparation();
            if (sep.sqrMagnitude > 0.0001f)
            {
                // Blend goal direction with separation, re-normalize
                dir = (dir + sep).normalized;
            }
        }

        transform.position += (Vector3)(dir * moveSpeed * Time.deltaTime);
    }

    Vector2 ComputeSeparation()
    {
        var hits = Physics2D.OverlapCircleAll(transform.position, separationRadius, allyMask);
        if (hits == null || hits.Length == 0) return Vector2.zero;

        Vector2 total = Vector2.zero;
        int counted = 0;

        for (int i = 0; i < hits.Length; i++)
        {
            var h = hits[i];
            if (!h) continue;

            var other = h.GetComponentInParent<UnitBrain>();
            if (!other || other == this) continue;
            if (other.isPlayerUnit != this.isPlayerUnit) continue;  
            Vector2 diff = (Vector2)(transform.position - other.transform.position);
            float d = diff.magnitude;
            if (d < 0.0001f) continue;

            total += diff / (d * d);

            counted++;
            if (counted >= separationMaxNeighbors) break; 
        }

        if (counted == 0) return Vector2.zero;

        total /= counted;                
        total = total.normalized * separationForce;
        return total;
    }

    void Face(bool faceRight)
    {
        if (!anim)
        { // still handle visual flip via SpriteRenderer if no Animator
            var sr = GetComponentInChildren<SpriteRenderer>();
            if (sr) sr.flipX = faceRight;
            return;
        }
        var sr2 = anim.GetComponent<SpriteRenderer>() ?? GetComponentInChildren<SpriteRenderer>();
        if (sr2) sr2.flipX = faceRight;
    }

   public void SetAnim(bool moving, bool dead)
    {
        if (!anim) return;
        if (!string.IsNullOrEmpty(isMovingParam)) anim.SetBool(isMovingParam, moving);
        if (!string.IsNullOrEmpty(isDeadParam)) anim.SetBool(isDeadParam, dead);
    }

    Transform FindClosestEnemy()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, sightRange, unitMask);
        float best = float.PositiveInfinity;
        Transform bestT = null;

        foreach (var h in hits)
        {
            if (!h || h.transform == transform) continue;
            var ub = h.GetComponentInParent<UnitBrain>();
            if (!ub || ub.isPlayerUnit == this.isPlayerUnit) continue;

            if (onlyInFront)
            {
                float dx = (ub.transform.position.x - transform.position.x) * (isPlayerUnit ? 1f : -1f);
                if (dx < 0) continue;
            }

            float d = (ub.transform.position - transform.position).sqrMagnitude;
            if (d < best) { best = d; bestT = ub.transform; }
        }

        if (!bestT && _stance == ArmyStance.Attack)
            bestT = foeTent ? foeTent : null;

        return bestT;
    }

    bool InAttackRange(Transform t)
    {
        if (!t) return false;
        float r = attackRange;
        return (t.position - transform.position).sqrMagnitude <= r * r;
    }

    void TryAttack(Transform t)
    {
        if (Time.time < nextAttackTime) return;

        if (!isRanged && armedMelee) return; 

        if (anim && !string.IsNullOrEmpty(attackTrigger)) anim.SetTrigger(attackTrigger);

        if (isRanged)
        {
            armed = true;
            attackTarget = t;
        }
        else
        {
            armedMelee = true;
            meleeTarget = t;
        }

        nextAttackTime = Time.time + (attackRate > 0 ? 1f / attackRate : 0f);
    }

    public void Anim_SpawnProjectile()
    {
        if (!armed) return;
        armed = false;

        if (!projectilePrefab || !muzzle || !attackTarget) return;

        Vector2 dir = (attackTarget.position - muzzle.position);
        if (dir.sqrMagnitude < 0.0001f) dir = new Vector2(isPlayerUnit ? 1f : -1f, 0f);
        dir = dir.normalized;

        var p = Instantiate(projectilePrefab, muzzle.position, Quaternion.identity);
        p.Init(attackDamage, dir * projectileSpeed, isPlayerUnit ? "Enemy" : "Player");
    }
    void FireProjectile(Transform t)
    {
        if (!projectilePrefab || !muzzle || !t) return;

        Vector2 dir = (t.position - muzzle.position);
        if (dir.sqrMagnitude < 0.0001f) dir = new Vector2(isPlayerUnit ? 1f : -1f, 0f);
        dir = dir.normalized;

        var p = Instantiate(projectilePrefab, muzzle.position, Quaternion.identity);
        p.Init(attackDamage, dir * projectileSpeed, isPlayerUnit ? "Enemy" : "Player");
    }
    public void Anim_MeleeHit()
    {
        if (!armedMelee) return;
        armedMelee = false;

        var t = meleeTarget;
        meleeTarget = null;

        if (!t) return;

        float slack = 0.15f; 
        float r = attackRange + slack;

        if ((t.position - transform.position).sqrMagnitude <= r * r)
        {
            var hp2 = t.GetComponentInParent<UnitHealth>();
            if (hp2 && !hp2.IsDead)
            {
                hp2.TakeDamage(attackDamage);
            }
        }
        
    }

    public void Anim_Destroy()
    {
        Destroy(gameObject);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, sightRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}