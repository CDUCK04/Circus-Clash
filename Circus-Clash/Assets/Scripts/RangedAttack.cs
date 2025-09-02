using UnityEngine;
using CircusClash.Troops.Common;
public class RangedAttack : MonoBehaviour
{
    [Header("References")]
    public Projectile2D projectilePrefab;
    public Transform muzzle;

    [Header("Timing")]
    public float cooldownOverride = 0f;

    [Header("Animation Integration")]
    [Tooltip("If true, TryShoot() only ARMS the shot. The projectile spawns when Anim_Fire() is called by an Animation Event on this same GameObject.")]
    public bool requireAnimationEvent = true;

    UnitStats stats;
    float nextReady;

    Transform queuedTarget;
    int queuedFacing;
    bool armed;

    void Awake() => stats = GetComponent<UnitStats>();

    bool Ready => Time.time >= nextReady;

    public bool TryShoot(Transform target, bool faceRight)
    {
        if  (target == null) return false;

        float r = stats ? stats.AttackRange : 6f;
        if ((target.position - transform.position).sqrMagnitude > r * r) return false;

        if (requireAnimationEvent)
        {
            queuedTarget = target;
            queuedFacing = faceRight ? +1 : -1;
            armed = true;
            return true;
        }
        else
        {
            return FireNow(target, faceRight);
        }
    }
   
    public void Anim_Fire()
    {
        if (!requireAnimationEvent) return;

        if (!armed) return;

        Transform t = queuedTarget;
        bool faceRight = queuedFacing > 0;

        if (t != null)
        {
            FireNow(t, faceRight);
        }
        else
        {
            FireStraight(faceRight);
        }

        armed = false;
        queuedTarget = null;
        queuedFacing = 0;
    }

    bool FireNow(Transform target, bool faceRight)
    {
        if (projectilePrefab == null || muzzle == null) return false;

        int dmg = stats ? stats.AttackDamage : 6;
        float spd = stats ? stats.ProjectileSpeed : 8f;

        var p = Instantiate(projectilePrefab, muzzle.position, Quaternion.identity);
        var dir = faceRight ? +1f : -1f;
        p.Init(dmg, new Vector2(spd * dir, 0f));

        return true;
    }

    void FireStraight(bool faceRight)
    {
        if (projectilePrefab == null || muzzle == null) return;

        int dmg = stats ? stats.AttackDamage : 6;
        float spd = stats ? stats.ProjectileSpeed : 8f;
        var p = Instantiate(projectilePrefab, muzzle.position, Quaternion.identity);
        p.Init(dmg, new Vector2(spd * (faceRight ? +1f : -1f), 0f));

        float cd = cooldownOverride > 0 ? cooldownOverride :
                   (stats && stats.AttackRate > 0 ? 1f / stats.AttackRate : 0.6f);
        nextReady = Time.time + cd;
    }
}