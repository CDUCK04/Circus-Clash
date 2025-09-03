using System;
using UnityEngine;

public class EnemyCommander : MonoBehaviour
{
    public static EnemyCommander Instance;

    [Header("Defaults")]
    public ArmyStance startingStance = ArmyStance.Attack;

    public ArmyStance CurrentStance { get; private set; }
    public event Action<ArmyStance> OnStanceChanged;

    void Awake()
    {
        if (Instance && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        CurrentStance = startingStance;
    }

    public void SetStance(ArmyStance s)
    {
        if (CurrentStance == s) return;
        CurrentStance = s;
        OnStanceChanged?.Invoke(s);
    }
}
