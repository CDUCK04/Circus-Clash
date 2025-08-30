using UnityEngine;
using CircusClash.Troops.Common; // for UnitStats

namespace CircusClash.Troops.Movement
{
    [RequireComponent(typeof(UnitStats))]
    public class UnitMover2D : MonoBehaviour
    {
        [Tooltip("Player troops move +X; enemy troops move -X.")]
        public bool isPlayerSide = true;

        // Property (PascalCase) so other scripts can read but not set it directly
        public bool IsStopped { get; private set; }

        private UnitStats unitStats; // renamed to avoid ambiguity
        private Vector2 moveDir;

        void Awake()
        {
            unitStats = GetComponent<UnitStats>();
            if (unitStats == null)
            {
                Debug.LogError($"{name}: UnitStats missing.", this);
            }

            // cache direction once (flip via SetSide if needed at runtime)
            moveDir = isPlayerSide ? Vector2.right : Vector2.left;
        }

        void Update()
        {
            if (IsStopped || unitStats == null) return;

            // Uses your API: MoveSpeed (float, units/sec)
            transform.Translate(moveDir * unitStats.MoveSpeed * Time.deltaTime);
        }

        public void Stop() => IsStopped = true;
        public void Resume() => IsStopped = false;

        // Optional: call this if you ever need to flip sides at runtime
        public void SetSide(bool playerSide)
        {
            isPlayerSide = playerSide;
            moveDir = isPlayerSide ? Vector2.right : Vector2.left;
        }
    }
}
