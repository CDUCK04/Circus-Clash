using UnityEngine;

namespace CircusClash.Troops.Combat
{
    public class DebugHotkeyDamage : MonoBehaviour
    {
        public int damage = 25;
        public KeyCode key = KeyCode.H;

        void Update()
        {
            if (Input.GetKeyDown(key))
            {
                var hp = GetComponent<UnitHealth>();
                if (hp != null) hp.TakeDamage(damage);
                Debug.Log($"{name} took {damage}. Current HP: {hp?.Current}");
            }
        }
    }
}
