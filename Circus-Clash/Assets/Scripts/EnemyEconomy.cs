using UnityEngine;

public class EnemyEconomy : MonoBehaviour
{
    [Header("Tickets")]
    public int tickets = 50;

    [Tooltip("Base income per tick before upgrades.")]
    public int baseIncomePerTick = 2;

    [Tooltip("Seconds between income ticks.")]
    public float tickSeconds = 1.5f;

    [Header("Booth Upgrade")]
    public int boothLevel = 1;
    [Tooltip("Income gained per booth level.")]
    public int incomePerLevel = 1;
    [Tooltip("Base cost of the first upgrade.")]
    public int baseUpgradeCost = 40;
    [Tooltip("Added cost per additional upgrade.")]
    public int upgradeCostStep = 25;
    [Tooltip("Max level (optional). 0 = unlimited.")]
    public int maxBoothLevel = 0;

    float nextTick;

    void Update()
    {
        if (Time.time >= nextTick)
        {
            nextTick = Time.time + tickSeconds;
            tickets += GetIncomePerTick();
        }
    }

    public int GetIncomePerTick()
    {
        int extra = Mathf.Max(0, boothLevel - 1) * incomePerLevel;
        return baseIncomePerTick + extra;
    }

    public int GetUpgradeCost()
    {
        int steps = Mathf.Max(0, boothLevel - 1);
        return baseUpgradeCost + steps * upgradeCostStep;
    }

    public bool CanUpgradeBooth()
    {
        if (maxBoothLevel > 0 && boothLevel >= maxBoothLevel) return false;
        return tickets >= GetUpgradeCost();
    }

    public bool TryUpgradeBooth()
    {
        if (!CanUpgradeBooth()) return false;
        int cost = GetUpgradeCost();
        tickets -= cost;
        boothLevel++;
        return true;
    }

    public bool TrySpend(int cost)
    {
        if (tickets < cost) return false;
        tickets -= cost;
        return true;
    }
}