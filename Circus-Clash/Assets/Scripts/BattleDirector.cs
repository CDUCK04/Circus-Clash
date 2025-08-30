using System;
using UnityEngine;

public enum ArmyStance { Attack, Defend, Retreat }

public class BattleDirector : MonoBehaviour
{
    public static BattleDirector Instance;

    [Header("Scene Anchors")]
    public Transform playerTent;
    public Transform enemyTent;    
    public float tentStopRadius = 0.8f;

    [Header("Defaults")]
    public ArmyStance startingStance = ArmyStance.Defend;

    public ArmyStance CurrentStance { get; private set; }

    public event Action<ArmyStance> OnStanceChanged;

    void Awake()
    {
        if (Instance && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        CurrentStance = startingStance;
    }
    public void CmdAttack() => SetStance(ArmyStance.Attack);
    public void CmdDefend() => SetStance(ArmyStance.Defend);
    public void CmdRetreat() => SetStance(ArmyStance.Retreat);

    public void SetStance(ArmyStance s)
    {
        if (CurrentStance == s) return;
        CurrentStance = s;
        OnStanceChanged?.Invoke(CurrentStance);
    }
}
