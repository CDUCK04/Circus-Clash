using UnityEngine;
using CircusClash.Troops.Movement; // for UnitMover2D

namespace CircusClash.Troops.AI
{
    /// <summary>
    /// Uses UnitSensor2D to automatically stop the mover
    /// when an enemy is within attack range.
    /// </summary>
    [RequireComponent(typeof(UnitSensor2D))]
    [RequireComponent(typeof(UnitMover2D))]
    public class AutoStopAtRange : MonoBehaviour
    {
        private UnitSensor2D sensor;
        private UnitMover2D mover;
        private Transform target; // enemy we are watching

        void Awake()
        {
            sensor = GetComponent<UnitSensor2D>();
            mover = GetComponent<UnitMover2D>();
        }

        void Update()
        {
            // If no target yet, look for one
            if (target == null)
                target = sensor.FindClosestEnemy();

            // If still no target, keep walking
            if (target == null)
            {
                mover.Resume();
                return;
            }

            // If target got destroyed or disappeared, forget it
            if (!target)
            {
                target = null;
                mover.Resume();
                return;
            }

            // Stop if enemy is close enough, else keep walking
            if (sensor.InAttackRange(target))
                mover.Stop();
            else
                mover.Resume();
        }

    }

}