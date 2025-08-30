using UnityEngine;
using CircusClash.Troops.Common;
using CircusClash.Troops.Combat;
using CircusClash.Troops.Movement;

namespace CircusClash.Troops.AI
{
    public class UnitAI2D : MonoBehaviour
    {
        private enum State { Advance, Attack, Dead }

        private State state;
        private UnitHealth health;
        private MeleeAttack melee;
        private UnitSensor2D sensor;      // your existing script
        private AutoStopAtRange stopper;   // your existing script (movement authority)
        private UnitMover2D mover;         // read-only checks if needed
        private RangedAttack ranged;
        private UnitStats stats;

        private Transform target;          // cached target we’re working with

        void Awake()
        {
            health = GetComponent<UnitHealth>();
            melee = GetComponent<MeleeAttack>();
            ranged = GetComponent<RangedAttack>();
            stats = GetComponent<UnitStats>();
            sensor = GetComponent<UnitSensor2D>();
            stopper = GetComponent<AutoStopAtRange>();
            mover = GetComponent<UnitMover2D>();
            state = State.Advance;
        }

        void Update()
        {
            if (health == null || health.IsDead)
            {
                EnterDead();
                return;
            }

            // Refresh/validate target each frame
            if (target == null || !target)
            {
                target = sensor != null ? sensor.FindClosestEnemy() : null;
            }

            switch (state)
            {
                case State.Advance:
                    TickAdvance();
                    break;
                case State.Attack:
                    TickAttack();
                    break;
            }
        }

        void TickAdvance()
        {
            // If no target, just keep advancing; AutoStopAtRange will handle stopping when needed
            if (target == null) return;

            // When we’re in range, switch to Attack; range check via your sensor API
            if (sensor != null && sensor.InAttackRange(target))
            {
                state = State.Attack;
            }
        }

        void TickAttack()
        {
            // Lost target? Go back to Advance.
            if (target == null || !target)
            {
                state = State.Advance;
                return;
            }

            // If target died, drop it and return to Advance to seek a new one
            var th = target.GetComponentInParent<UnitHealth>();
            if (th == null || th.IsDead)
            {
                target = null;
                state = State.Advance;
                return;
            }

            // If out of range, let AutoStopAtRange resume movement; AI goes back to Advance state
            if (sensor != null && !sensor.InAttackRange(target))
            {
                state = State.Advance;
                return;
            }

            // In range: try to attack (handles cooldown internally)
            bool usedRanged = false;
            if (ranged != null || (stats != null && stats.IsRanged)) // prefer ranged when present
            {
                int facing = transform.localScale.x >= 0 ? +1 : -1;
                usedRanged = ranged != null && ranged.TryShoot(target, facing > 0);
            }

            if (!usedRanged && melee != null)
            {
                melee.TryAttack(target); // fallback to melee
            }
        }

        void EnterDead()
        {
            state = State.Dead;
            // Movement & destruction are handled elsewhere:
            // - AutoStopAtRange + UnitMover2D won’t matter after death
            // - UnitHealth.Die() destroys the GameObject
        }
    }
}
