using UnityEngine;
using UnityEngine.Events;
using CircusClash.Troops.Common; // for UnitStats

namespace CircusClash.Troops.Combat
{
    [RequireComponent(typeof(UnitStats))]
    public class UnitHealth : MonoBehaviour
    {
        [Tooltip("Fires when HP hits 0 (coins, SFX, remove unit, etc).")]
        public UnityEvent onDied;

        // inside class:
        [Tooltip("Fires when any damage is successfully applied. Int = damage amount.")]
        public UnityEvent<int> onDamaged;

        public int Current { get; private set; }
        public bool IsDead => Current <= 0;

        private UnitStats stats;

        void Awake()
        {
            stats = GetComponent<UnitStats>();
            if (stats == null)
            {
                Debug.LogError($"{name}: UnitStats missing.");
                Current = 1;
                return;
            }

            Current = Mathf.Max(1, stats.MaxHealth);
            Debug.Log($"{name} spawned with HP: {Current}");
        }

        public void TakeDamage(int amount)
        {
            if (IsDead) return;

            amount = Mathf.Max(0, amount); // no negative damage
            Current -= amount;

            onDamaged?.Invoke(amount);

            if (Current <= 0)
            {
                Current = 0;
                Die();
            }
        }

        public void Heal(int amount)
        {
            if (IsDead) return;

            amount = Mathf.Max(0, amount);
            Current = Mathf.Min(Current + amount, stats.MaxHealth);
        }

        private void Die()
        {
            onDied?.Invoke();
            Destroy(gameObject); // placeholder: pooling/anim later
        }
    }

}
