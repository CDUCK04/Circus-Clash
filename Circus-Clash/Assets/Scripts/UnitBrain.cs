using System.Collections;
using System.Collections.Generic;
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

    public bool IsEngaged { get; private set; }

    [Header("Pursue Relaxation")]
    public float defendPursueRange = 0.75f; 

    [Header("Anti-Jam (ally pass-through)")]
    public bool enableAllyPassThrough = true;
    public float blockedSpeedThreshold = 0.00002f; 
    public float blockedCheckSeconds = 0.6f;    
    public float passProbeDistance = 0.9f;        
    public float passProbeRadius = 0.3f;           
    public float passThroughSeconds = 0.6f;        

    float _blockedTimer;
    bool _isPassing;
    readonly System.Collections.Generic.HashSet<(Collider2D a, Collider2D b)> _ignoredPairs
        = new System.Collections.Generic.HashSet<(Collider2D, Collider2D)>();

    Vector3 _lastPos;
    float _movedThisFrame;
    UnitHealth hp;

    // Tents & formation
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
    BattleDirector _playerDir;
    EnemyCommander _enemyCmd;

    bool _stopped;
    Vector2 MoveDir => isPlayerUnit ? Vector2.right : Vector2.left;

    void Awake()
    {
        hp = GetComponent<UnitHealth>();
        formation = FindObjectOfType<FormationManager>();
        if (startStopped) _stopped = true;
        _lastPos = transform.position;
    }

    void Start()
    {
        _playerDir = BattleDirector.Instance;
        _enemyCmd = EnemyCommander.Instance;

        if (isPlayerUnit)
        {
            if (_playerDir)
            {
                foeTent = _playerDir.enemyTent;
                _playerDir.OnStanceChanged += OnPlayerStanceChanged;
                ApplyStance(_playerDir.CurrentStance);
            }
            else
            {
                ApplyStance(ArmyStance.Retreat);
            }
        }
        else
        {
            if (_playerDir) foeTent = _playerDir.playerTent;

            if (_enemyCmd)
            {
                _enemyCmd.OnStanceChanged += OnEnemyStanceChanged;
                ApplyStance(_enemyCmd.CurrentStance);
            }
            else
            {
                ApplyStance(ArmyStance.Attack);
            }
        }
    }

    void OnDestroy()
    {
        if (isPlayerUnit && _playerDir) _playerDir.OnStanceChanged -= OnPlayerStanceChanged;
        if (!isPlayerUnit && _enemyCmd) _enemyCmd.OnStanceChanged -= OnEnemyStanceChanged;

        if (formation && isPlayerUnit) formation.Unregister(this);
    }

    public void SetFormationSlot(Vector3 worldPos, int row, int col)
    {
        homeSlot = worldPos;
        homeSlotSet = true;
    }

    void OnPlayerStanceChanged(ArmyStance s)
    {
        if (isPlayerUnit) ApplyStance(s);
    }

    void OnEnemyStanceChanged(ArmyStance s)
    {
        if (!isPlayerUnit) ApplyStance(s);
    }

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

        target = FindClosestEnemy();
        bool inRange = target && InAttackRange(target);

        Vector3 dest = transform.position;
        var stance = _stance;

        bool allowPursue = (stance == ArmyStance.Attack) ||
                           (stance == ArmyStance.Defend && target && Vector2.Distance(transform.position, target.position) <= defendPursueRange);

        if (pursueTargetInSight && allowPursue && target && !inRange)
        {
            dest = target.position;
        }
        else
        {
            if (stance == ArmyStance.Attack)
            {
                float yLine = homeSlotSet ? homeSlot.y : transform.position.y;
                float xGoal = foeTent ? foeTent.position.x : transform.position.x;
                dest = new Vector3(xGoal, yLine, transform.position.z);
            }
            else if (stance == ArmyStance.Retreat)
            {
                dest = homeSlotSet ? homeSlot : transform.position;
            }
            else 
            {
                dest = holdingHere ? holdPoint : transform.position;
            }
        }

        if (inRange)
        {
            _stopped = true;
            TryAttack(target);
        }
        else
        {
            if (stance == ArmyStance.Defend && (!target || !allowPursue))
            {
                _stopped = true;
            }
            else
            {
                _stopped = false;
                bool toAnchor = (stance == ArmyStance.Retreat) || (stance == ArmyStance.Defend);
                MoveToward(dest, disableSeparation: toAnchor);
            }
        }

        Face((target ? (target.position.x - transform.position.x) : (isPlayerUnit ? +1f : -1f)) >= 0f);

        IsEngaged = (target && inRange) || armed || armedMelee;

        bool movingForAnim = !_stopped && _movedThisFrame > 0.000001f;
        SetAnim(movingForAnim, false);

        if (enableAllyPassThrough && !_isPassing)
        {
            if (!_stopped && _movedThisFrame < blockedSpeedThreshold)
                _blockedTimer += Time.deltaTime;
            else
                _blockedTimer = 0f;

            if (_blockedTimer >= blockedCheckSeconds && AllyDirectlyAhead(out var allyColliders))
            {
                StartCoroutine(TemporaryPassThroughAllies(allyColliders, passThroughSeconds));
                _blockedTimer = 0f;
            }
        }

        _lastPos = transform.position;
    }

    bool AllyDirectlyAhead(out System.Collections.Generic.List<Collider2D> allyCols)
    {
        allyCols = null;

        Vector2 dir = new Vector2(isPlayerUnit ? 1f : -1f, 0f);
        Vector2 origin = (Vector2)transform.position + dir * (passProbeRadius * 0.5f);

        var hits = Physics2D.OverlapCircleAll(origin + dir * passProbeDistance * 0.5f,
                                              passProbeRadius, allyMask);
        if (hits == null || hits.Length == 0) return false;

        var list = new System.Collections.Generic.List<Collider2D>();
        foreach (var h in hits)
        {
            if (!h) continue;
            var ub = h.GetComponentInParent<UnitBrain>();
            if (!ub || ub == this) continue;
            if (ub.isPlayerUnit != this.isPlayerUnit) continue; 

            float dx = (ub.transform.position.x - transform.position.x) * (isPlayerUnit ? 1f : -1f);
            if (dx < -0.05f) continue;

            list.Add(h);
        }

        if (list.Count == 0) return false;
        allyCols = list;
        return true;
    }

    IEnumerator TemporaryPassThroughAllies(List<Collider2D> allyCols, float seconds)
    {
        _isPassing = true;

        var myCols = GetComponentsInChildren<Collider2D>();
        if (myCols != null && allyCols != null)
        {
            foreach (var mc in myCols)
            {
                if (!mc || !mc.enabled) continue;
                foreach (var ac in allyCols)
                {
                    if (!ac || !ac.enabled) continue;
                    Physics2D.IgnoreCollision(mc, ac, true);
                    _ignoredPairs.Add((mc, ac));
                }
            }
        }

        float t = 0f;
        while (t < seconds)
        {
            t += Time.unscaledDeltaTime; 
            yield return null;
        }

        foreach (var pair in _ignoredPairs)
        {
            if (pair.a && pair.b) Physics2D.IgnoreCollision(pair.a, pair.b, false);
        }
        _ignoredPairs.Clear();

        _isPassing = false;
    }

    void MoveToward(Vector3 dest, bool disableSeparation = false)
    {
        Vector3 to = dest - transform.position;
        float distSqr = to.sqrMagnitude;

        float stopSqr = stopDistance * stopDistance;
        if (distSqr <= stopSqr)
        {
            transform.position = dest;
            _stopped = true;
            _movedThisFrame = 0f;
            return;
        }

        Vector2 dir = new Vector2(to.x, to.y).normalized;

        if (!disableSeparation && enableSeparation)
        {
            Vector2 sep = ComputeSeparation();
            if (sep.sqrMagnitude > 0.0001f)
                dir = (dir + sep).normalized;
        }

        Vector3 before = transform.position;
        transform.position += (Vector3)(dir * moveSpeed * Time.deltaTime);
        _movedThisFrame = (transform.position - before).sqrMagnitude;
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
        {
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

        if (!string.IsNullOrEmpty(isMovingParam))
            anim.SetBool(isMovingParam, moving);

        if (!string.IsNullOrEmpty(isDeadParam))
            anim.SetBool(isDeadParam, dead);

        anim.SetBool("IsIdle", !moving && !IsEngaged && !dead);
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