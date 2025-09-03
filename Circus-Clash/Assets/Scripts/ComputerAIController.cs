using System.Linq;
using UnityEngine;

public class ComputerAIController : MonoBehaviour
{
    [Header("Refs")]
    public EnemyEconomy econ;
    public EnemySpawner spawner;
    public EnemyCommander commander;

    [Header("Unit Costs")]
    public int clownCost = 30;
    public int magicianCost = 50;

    [Header("Behavior")]
    public float thinkInterval = 1.0f;
    public float powerAttackFactor = 1.2f;  
    public float powerRetreatFactor = 0.7f; 
    public int minUnitsToPush = 4;        
    public float rangedWeight = 1.4f;       

    [Header("Purchasing")]
    public int targetUnitCount = 16;        
    public float desiredRangedRatio = 0.4f;

    void OnEnable() { InvokeRepeating(nameof(Think), thinkInterval, thinkInterval); }
    void OnDisable() { CancelInvoke(nameof(Think)); }

    void Think()
    {
        if (!econ || !spawner || !commander) return;

        var brains = FindObjectsOfType<UnitBrain>();
        var ours = brains.Where(b => b && !b.isPlayerUnit && !b.GetComponent<UnitHealth>().IsDead).ToList();
        var theirs = brains.Where(b => b && b.isPlayerUnit && !b.GetComponent<UnitHealth>().IsDead).ToList();

        float ourRanged = ours.Count(b => b.isRanged);
        float ourMelee = ours.Count - ourRanged;
        float theirRanged = theirs.Count(b => b.isRanged);
        float theirMelee = theirs.Count - theirRanged;

        float ourPower = ourMelee + ourRanged * rangedWeight;
        float theirPower = theirMelee + theirRanged * rangedWeight;

        if (ours.Count < targetUnitCount)
        {
            float currentRangedRatio = ours.Count == 0 ? 0f : (ourRanged / Mathf.Max(1f, ours.Count));
            bool needRanged = currentRangedRatio < desiredRangedRatio;

            if (needRanged && econ.TrySpend(magicianCost))
                spawner.SpawnMagicianEnemy();
            else if (econ.TrySpend(clownCost))
                spawner.SpawnClownEnemy();
        }
        else
        {
            if (econ.tickets >= magicianCost * 2)
            {
                econ.TryUpgradeBooth();
                econ.TrySpend(magicianCost);
            }
        }

        ArmyStance newStance = commander.CurrentStance;

        if (theirs.Count == 0 && ours.Count >= Mathf.Max(2, minUnitsToPush))
        {
            newStance = ArmyStance.Attack;
        }
        else
        {
            if (ours.Count < minUnitsToPush)
            {
                newStance = ArmyStance.Retreat;
            }
            else
            {
                float ratio = (theirPower <= 0.01f) ? 999f : (ourPower / theirPower);
                if (ratio >= powerAttackFactor)
                    newStance = ArmyStance.Attack;
                else if (ratio <= powerRetreatFactor)
                    newStance = ArmyStance.Retreat;
                else
                    newStance = ArmyStance.Defend;
            }
        }

        if (newStance != commander.CurrentStance)
            commander.SetStance(newStance);
    }
}