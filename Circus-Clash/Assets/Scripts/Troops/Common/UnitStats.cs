using UnityEngine;

namespace CircusClash.Troops.Common
{
    [DisallowMultipleComponent]
    [AddComponentMenu("CircusClash/Troops/Unit Stats")]
    public class UnitStats : MonoBehaviour
    {
        [Header("Link your ScriptableObject here")]
        public UnitStatsSO stats;

        public int MaxHealth => stats ? stats.maxHealth : 100;
        public float MoveSpeed => stats ? stats.moveSpeed : 1.5f;
        public int AttackDamage => stats ? stats.attackDamage : 10;
        public float AttackRate => stats ? stats.attackRate : 1f; // seconds/attack
        public float AttackRange => stats ? stats.attackRange : 1f;
        public int Cost => stats ? stats.cost : 0;
        public int PopCost => stats ? stats.popCost : 1;
        public int TicketReward => stats ? stats.ticketReward : 0;
        public bool IsRanged => stats && stats.isRanged;
        public float ProjectileSpeed => stats ? stats.projectileSpeed : 0f;

        // Editor-time nudge to avoid silent defaults forever
        void OnValidate()
        {
            if (stats == null)
            {
                // This shows a yellow warning in the Console during edit time.
                Debug.LogWarning($"{name}: UnitStatsSO is not assigned on UnitStats. Using fallback defaults.", this);
            }
        }
    }
}
