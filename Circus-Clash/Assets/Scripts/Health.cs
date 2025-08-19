using UnityEngine;
using UnityEngine.Events;

public class Health : MonoBehaviour
{
    public int maxHP = 100;
    public int currentHP;
    public UnityEvent onDeath;

    void Awake() => currentHP = maxHP;

    public void TakeDamage(int amount)
    {
        if (currentHP <= 0) return;
        currentHP -= amount;
        if (currentHP <= 0)
        {
            currentHP = 0;
            onDeath?.Invoke();
            Destroy(gameObject); // simple for now
        }
    }
}

