using UnityEngine;
using UnityEngine.Events;
using CircusClash.Troops.Common;

namespace CircusClash.Troops.Combat
{
    public class MeleeAttack : MonoBehaviour
    {
        [Tooltip("Optional override. If zero or negative, uses UnitStats.AttackDamage.")]
        public int damageOverride = 0;

        [Tooltip("Optional override. If zero or negative, uses 1f / UnitStats.AttackRate.")]
        public float cooldownOverride = 0f;

        [Tooltip("How far we can hit (used if your AutoStopAtRange doesn’t enforce exact stopping).")]
        public float attackRange = 1.5f;

        // Inside class MeleeAttack
        [Tooltip("Fires when an attack actually lands. Int = damage dealt.")]
        public UnityEvent<int> onHit;


        private UnitStats stats;
        private float nextReadyTime;

        void Awake()
        {
            stats = GetComponent<UnitStats>();
        }

        public bool IsReady => Time.time >= nextReadyTime;

        public bool TryAttack(Transform target)
        {
            if (target == null || !IsReady) return false;

            // distance check for safety
            if ((target.position - transform.position).sqrMagnitude > attackRange * attackRange)
                return false;

            var targetHealth = target.GetComponentInParent<UnitHealth>();
            if (targetHealth == null || targetHealth.IsDead) return false;

            int dmg = damageOverride > 0 ? damageOverride : (stats != null ? stats.AttackDamage : 1);
            targetHealth.TakeDamage(dmg);
            onHit?.Invoke(dmg);


            float cd = cooldownOverride > 0 ? cooldownOverride
                : (stats != null && stats.AttackRate > 0 ? 1f / stats.AttackRate : 0.75f);
            nextReadyTime = Time.time + cd;
            return true;
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.DrawWireSphere(transform.position, attackRange);
        }
    }


}