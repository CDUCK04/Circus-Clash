using UnityEngine;

namespace CircusClash.Troops.Common
{
    [CreateAssetMenu(
        fileName = "UnitStats",
        menuName = "CircusClash/Unit Stats",
        order = 0
    )]

    public class UnitStatsSO : ScriptableObject
    {
        [Header("Identity")]
        public string unitName = "Clown";

        [Header("Vitals")]
        [Min(1)] public int maxHealth = 100;

        [Header("Movement")]
        [Min(0f)] public float moveSpeed = 1.5f;

        [Header("Combat")]
        [Tooltip("Damage done per successful hit.")]
        [Min(0)] public int attackDamage = 10;

        [Tooltip("Seconds between attacks (e.g., 0.8s).")]
        [Min(0.05f)] public float attackRate = 0.8f;

        [Tooltip("World units. ~1.0–1.5 for melee; bigger for ranged.")]
        [Min(0f)] public float attackRange = 1.5f;

        [Header("Economy")]
        [Min(0)] public int cost = 25;
        [Min(0)] public int popCost = 1;
        [Min(0)] public int ticketReward = 5;

        [Header("Ranged (if applicable)")]
        public bool isRanged = false;

        [Tooltip("Only used if isRanged=true.")]
        [Min(0f)] public float projectileSpeed = 6f;

#if UNITY_EDITOR
        // Editor-side safety net; doesn't run in builds
        private void OnValidate()
        {
            if (!isRanged) projectileSpeed = 0f;
        }
#endif
    }

}
