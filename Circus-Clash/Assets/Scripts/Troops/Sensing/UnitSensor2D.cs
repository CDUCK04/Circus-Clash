using UnityEngine;
using CircusClash.Troops.Movement; // for UnitMover2D

namespace CircusClash.Troops.AI
{
    /// <summary>
    /// 2D unit sensor that:
    /// - Finds the closest enemy in front within sightRange.
    /// - Checks whether a target is within attackRange.
    /// Uses layer filtering so we only consider "Units".
    /// </summary>
    [RequireComponent(typeof(UnitMover2D))]
    public class UnitSensor2D : MonoBehaviour
    {
        [Header("Ranges")]
        [Tooltip("How far this unit can see potential targets (yellow gizmo).")]
        [Min(0f)] public float sightRange = 6f;

        [Tooltip("Distance at which we consider the target close enough to stop/attack (red gizmo).")]
        [Min(0f)] public float attackRange = 1.2f;

        [Header("Filtering")]
        [Tooltip("Physics LayerMask used to limit detection to units (set to 'Units').")]
        public LayerMask unitLayer;


        [Header("Debug")]
        public bool debugLogs = true;

        [Tooltip("If true, only consider targets in front (+X for player, -X for enemy).")]
        public bool onlyInFront = true;

        // Cached dependency
        private UnitMover2D mover;

        private void Awake()
        {
            mover = GetComponent<UnitMover2D>();
            if (!mover)
            {
                Debug.LogError($"{name}: UnitMover2D missing (required for direction).", this);
            }

            if (debugLogs)
            {
                Debug.Log($"{name}: UnitSensor2D awake. unitLayer={unitLayer.value}, sight={sightRange}, attack={attackRange}");
            }
        }

        /// <summary>
        /// Finds the nearest enemy Transform within sightRange that passes filters.
        /// Enemy = has UnitMover2D and opposite isPlayerSide.
        /// Optionally ignores anything behind us on the X axis.
        /// Returns null if nothing suitable found.
        /// </summary>
        public Transform FindClosestEnemy()
        {
            // Collect nearby colliders on the specified unit layer.
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, sightRange, unitLayer);
            if (debugLogs) Debug.Log($"{name}: OverlapCircleAll found {hits.Length} hits on layer mask {unitLayer.value}");


            if (hits == null || hits.Length == 0) return null;

            // Determine our "forward" sign along X (+1 for player, -1 for enemy).
            // If mover is missing, default to +1 (player-like) to avoid crashes.
            float forwardSign = (mover != null && mover.isPlayerSide) ? +1f : -1f;

            Transform best = null;
            float bestSqr = float.PositiveInfinity;

            Vector3 myPos = transform.position;

            foreach (Collider2D h in hits)
            {
                if (!h) { if (debugLogs) Debug.Log($"{name}: hit null collider, skipping"); continue; }

                // Must be a unit (has UnitMover2D)
                UnitMover2D otherMover = h.GetComponent<UnitMover2D>();
                if (!otherMover)
                {
                    if (debugLogs) Debug.Log($"{name}: {h.name} has no UnitMover2D (not a unit), skipping.");
                    continue;
                }

                // Must be on the opposite side (enemy)
                if (mover != null && otherMover.isPlayerSide == mover.isPlayerSide)
                {
                    if (debugLogs) Debug.Log($"{name}: {h.name} is same side, skipping.");
                    continue;
                }
                // we only care about what's in front, discard behind us.
                if (onlyInFront)
                {
                    float dx = (h.transform.position.x - myPos.x) * forwardSign;
                    if (dx < 0f)
                    {
                        if (debugLogs) Debug.Log($"{name}: {h.name} is behind me, skipping.");
                        continue;
                    }
                }

                // Keep the nearest target by squared distance
                float sqr = (h.transform.position - myPos).sqrMagnitude;
                if (debugLogs) Debug.Log($"{name}: candidate {h.name}, sqrDist={sqr:F3}");

                if (sqr < bestSqr)
                {
                    bestSqr = sqr;
                    best = h.transform;
                }
            }

            if (debugLogs) Debug.Log(best ? $"{name}: BEST target = {best.name}" : $"{name}: no valid target");
            return best;
        }

        /// <summary>
        /// Returns true if 'target' is within attackRange of this unit.
        /// Uses squared distance for performance (avoids sqrt).
        /// </summary>
        public bool InAttackRange(Transform target)
        {
            if (!target) return false;
            float sqr = (target.position - transform.position).sqrMagnitude;
            bool inside = sqr <= attackRange * attackRange;
            if (debugLogs) Debug.Log($"{name}: InAttackRange({target.name})? {inside} (sqrDist={sqr:F3}, thresh={attackRange * attackRange:F3})");
            return inside;
        }

#if UNITY_EDITOR
        /// <summary>
        /// Draws sight (yellow) and attack (red) range gizmos when selected in Scene view.
        /// </summary>
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, sightRange);

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);
        }
#endif
    }
}
