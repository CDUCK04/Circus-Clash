using UnityEngine;
using CircusClash.Troops.Common;

public class RangedAttack : MonoBehaviour
{
    public Projectile2D projectilePrefab;
    public Transform muzzle;
    public float cooldownOverride = 0f;

    UnitStats stats;
    float nextReady;

    void Awake() => stats = GetComponent<UnitStats>();
    bool Ready => Time.time >= nextReady;

    public bool TryShoot(Transform target, bool faceRight)
    {
        if (!Ready || projectilePrefab == null || muzzle == null || target == null) return false;

        float r = stats ? stats.AttackRange : 6f;
        if ((target.position - transform.position).sqrMagnitude > r * r) return false;

        int dmg = stats ? stats.AttackDamage : 6;
        float spd = stats ? stats.ProjectileSpeed : 8f; 

        var p = Instantiate(projectilePrefab, muzzle.position, Quaternion.identity);
        var dir = (target.position.x >= transform.position.x) ? +1f : -1f;
        p.Init(dmg, new Vector2(spd * dir, 0f));

        float cd = cooldownOverride > 0 ? cooldownOverride :
                   (stats && stats.AttackRate > 0 ? 1f / stats.AttackRate : 0.6f);
        nextReady = Time.time + cd;
        return true;
    }
}