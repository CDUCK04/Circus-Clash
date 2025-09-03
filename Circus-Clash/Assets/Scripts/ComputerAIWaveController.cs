using System.Collections;
using System.Linq;
using UnityEngine;

public class ComputerAIWaveController : MonoBehaviour
{
    public enum Phase { Prepare, Launch, Pressure, Regroup }

    [Header("Refs")]
    public EnemyEconomy econ;
    public EnemySpawner spawner;
    public EnemyCommander commander;

    [Header("Unit Costs (match your shop)")]
    public int clownCost = 30;
    public int magicianCost = 50;

    [Header("Wave Settings")]
    public float prepareDuration = 5f;    
    public int baseWaveBudget = 180;       
    public int budgetGrowthPerWave = 70;   
    public int minUnitsToPush = 3;         

    [Range(0f, 1f)] public float desiredRangedRatio = 0.35f; 
    public int targetUnitCount = 18;       

    [Header("Tactics")]
    public float rangedWeight = 1.3f;
    public float powerAttackFactor = 1.05f;
    public float powerRetreatFactor = 0.50f; 
    public float pulseAttackSeconds = 4.5f;  
    public float pulseDefendSeconds = 1.2f;  
    public int burstSpawn = 5;             

    [Header("Economy Strategy")]
    public float minPrepareReserve = 10f;   
    public bool prioritizeEarlyUpgrades = true;
    public int maxConsecutiveUpgrades = 1; 
    public int maxBoothLevelAggressive = 3; 

    [Header("Harass")]
    public bool enableHarass = true;
    public float harassInterval = 3.5f;      
    public float harassRangedChance = 0.35f; 

    int waveIndex = 0;
    Phase phase = Phase.Prepare;
    float phaseEndTime;
    int waveBudgetRemaining;
    int upgradesThisWave;

    Coroutine harassCo;

    void OnEnable()
    {
        StartCoroutine(AILoop());
        if (enableHarass) harassCo = StartCoroutine(HarassLoop());
    }
    void OnDisable()
    {
        StopAllCoroutines();
    }

    IEnumerator AILoop()
    {
        yield return new WaitForSeconds(0.5f);
        EnterPrepare();
        while (true)
        {
            switch (phase)
            {
                case Phase.Prepare: DoPrepare(); break;
                case Phase.Launch: DoLaunch(); break;
                case Phase.Pressure: yield return StartCoroutine(DoPressure()); break;
                case Phase.Regroup: DoRegroup(); break;
            }
            yield return null;
        }
    }
    IEnumerator HarassLoop()
    {
        var wait = new WaitForSeconds(harassInterval);
        while (true)
        {
            yield return wait;
            if (!econ || !spawner) continue;

            var ours = FindObjectsOfType<UnitBrain>().Where(b => b && !b.isPlayerUnit && !b.GetComponent<UnitHealth>().IsDead).ToList();
            if (ours.Count >= 20) continue;

            bool tryRanged = Random.value < harassRangedChance;
            if (tryRanged && econ.TrySpend(magicianCost)) { spawner.SpawnMagicianEnemy(); }
            else if (econ.TrySpend(clownCost)) { spawner.SpawnClownEnemy(); }
        }
    }
    void EnterPrepare()
    {
        phase = Phase.Prepare;
        phaseEndTime = Time.time + prepareDuration;
        waveBudgetRemaining = baseWaveBudget + waveIndex * budgetGrowthPerWave;
        upgradesThisWave = 0;
        commander.SetStance(ArmyStance.Retreat); 
    }

    void DoPrepare()
    {
        if (!econ || !spawner || !commander) { EnterPrepare(); return; }

        if (prioritizeEarlyUpgrades &&
            upgradesThisWave < maxConsecutiveUpgrades &&
            econ.boothLevel < maxBoothLevelAggressive &&
            econ.CanUpgradeBooth())
        {
            econ.TryUpgradeBooth();
            upgradesThisWave++;
            return;
        }

        var ours = FindObjectsOfType<UnitBrain>().Where(b => b && !b.isPlayerUnit && !b.GetComponent<UnitHealth>().IsDead).ToList();
        if (ours.Count < targetUnitCount && waveBudgetRemaining > Mathf.Min(clownCost, magicianCost))
        {
            float wantRanged = desiredRangedRatio * (ours.Count + 1);
            int currentRanged = ours.Count(b => b.isRanged);
            bool needRanged = currentRanged < wantRanged;

            int spendable = Mathf.Max(0, econ.tickets - Mathf.RoundToInt(minPrepareReserve));
            bool bought = false;

            if (needRanged && spendable >= magicianCost && waveBudgetRemaining >= magicianCost)
            {
                if (econ.TrySpend(magicianCost)) { spawner.SpawnMagicianEnemy(); waveBudgetRemaining -= magicianCost; bought = true; }
            }
            else if (spendable >= clownCost && waveBudgetRemaining >= clownCost)
            {
                if (econ.TrySpend(clownCost)) { spawner.SpawnClownEnemy(); waveBudgetRemaining -= clownCost; bought = true; }
            }

            if (bought) return;
        }

        bool timeUp = Time.time >= phaseEndTime;
        int alive = ours.Count;
        if ((timeUp && alive >= minUnitsToPush) || alive >= targetUnitCount - 2) // push even if slightly under target
        {
            phase = Phase.Launch;
        }
    }

    void DoLaunch()
    {
        int bursted = 0;
        while (bursted < burstSpawn)
        {
            if (bursted % 2 == 0 && econ.TrySpend(magicianCost)) { spawner.SpawnMagicianEnemy(); bursted++; }
            else if (econ.TrySpend(clownCost)) { spawner.SpawnClownEnemy(); bursted++; }
            else break;
        }

        commander.SetStance(ArmyStance.Attack);
        phase = Phase.Pressure;
    }

    IEnumerator DoPressure()
    {
        float pulseTimer = 0f;
        bool attacking = true;
        commander.SetStance(ArmyStance.Attack);

        while (true)
        {
            var brains = FindObjectsOfType<UnitBrain>();
            var ours = brains.Where(b => b && !b.isPlayerUnit && !b.GetComponent<UnitHealth>().IsDead).ToList();
            var theirs = brains.Where(b => b && b.isPlayerUnit && !b.GetComponent<UnitHealth>().IsDead).ToList();

            float ourR = ours.Count(b => b.isRanged);
            float ourM = ours.Count - ourR;
            float thR = theirs.Count(b => b.isRanged);
            float thM = theirs.Count - thR;

            float ourPower = ourM + ourR * rangedWeight;
            float theirPower = thM + thR * rangedWeight;

            var playerStance = BattleDirector.Instance ? BattleDirector.Instance.CurrentStance : ArmyStance.Defend;
            if (playerStance == ArmyStance.Retreat) commander.SetStance(ArmyStance.Attack);

            if (theirPower > 0.01f && ourPower / theirPower <= powerRetreatFactor)
            {
                commander.SetStance(ArmyStance.Retreat);
                phase = Phase.Regroup;
                yield break;
            }

            if (theirs.Count == 0)
            {
                commander.SetStance(ArmyStance.Attack);
                if (econ.TrySpend(clownCost)) spawner.SpawnClownEnemy();
            }
            else
            {
                pulseTimer += Time.deltaTime;
                if (attacking && pulseTimer >= pulseAttackSeconds)
                {
                    commander.SetStance(ArmyStance.Defend);
                    attacking = false;
                    pulseTimer = 0f;
                }
                else if (!attacking && pulseTimer >= pulseDefendSeconds)
                {
                    commander.SetStance(ArmyStance.Attack);
                    attacking = true;
                    pulseTimer = 0f;
                }

                if (ours.Count < 20)
                {
                    float wantRanged = desiredRangedRatio * (ours.Count + 1);
                    if (ours.Count(b => b.isRanged) < wantRanged && econ.TrySpend(magicianCost))
                        spawner.SpawnMagicianEnemy();
                    else if (econ.TrySpend(clownCost))
                        spawner.SpawnClownEnemy();
                }
            }

            yield return null;
        }
    }

    void DoRegroup()
    {
        commander.SetStance(ArmyStance.Retreat);
        StartCoroutine(RegroupAndRestart(2.0f)); 
        phase = Phase.Prepare;
    }

    IEnumerator RegroupAndRestart(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        waveIndex++;
        EnterPrepare();
    }

    void Start() => EnterPrepare();
}
