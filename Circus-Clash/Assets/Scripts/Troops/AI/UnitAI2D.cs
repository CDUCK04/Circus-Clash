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
        private UnitSensor2D sensor;      
        private AutoStopAtRange stopper;  
        private UnitMover2D mover;         
        private RangedAttack ranged;
        private UnitStats stats;

        private Transform target;         

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
            
            if (target == null) return;

            
            if (sensor != null && sensor.InAttackRange(target))
            {
                state = State.Attack;
            }
        }

        void TickAttack()
        {
            if (target == null || !target)
            {
                state = State.Advance;
                return;
            }


            var th = target.GetComponentInParent<UnitHealth>();
            if (th == null || th.IsDead)
            {
                target = null;
                state = State.Advance;
                return;
            }


            if (sensor != null && !sensor.InAttackRange(target))
            {
                state = State.Advance;
                return;
            }

            var animDriver = GetComponent<UnitAnimationDriver>();

            bool fired = false;

            if (ranged != null || (stats != null && stats.IsRanged))
            {
                int facing = transform.localScale.x >= 0 ? +1 : -1;
                if (ranged != null && ranged.TryShoot(target, facing > 0))
                {
                    animDriver?.PlayAttack();
                    fired = true;
                }
            }

            if (!fired && melee != null)
            {
                if (melee.IsReady && melee.TryAttack(target))
                {
                    animDriver?.PlayAttack();
                    fired = true;
                }
            }
        }

            void EnterDead()
            {
                state = State.Dead;
                
            }
        }
    }

