using UnityEngine;
using TMPro;

public class TicketBooth : MonoBehaviour
{
    [Header("Economy")]
    public int baseCost = 75;
    public float costMult = 1.35f; 
    public int incomePerLevel = 3;

    [Header("UI (optional)")]
    public TMP_Text levelLabel;
    public TMP_Text costLabel;

    int level = 0;

    void Start() { RefreshUI(); }

    int CurrentCost => Mathf.RoundToInt(baseCost * Mathf.Pow(costMult, level));

    public void BtnUpgrade()
    {
        var cur = CurrencyManager.Instance;
        if (cur == null) return;

        int cost = CurrentCost;
        if (!cur.CanAfford(cost)) return;

        cur.Spend(cost);
        level++;

        cur.GetType().GetField("ticketsPerTick").SetValue(cur, cur.ticketsPerTick + incomePerLevel);

        RefreshUI();
    }

    void RefreshUI()
    {
        if (levelLabel) levelLabel.text = $"{level}";
        if (costLabel) costLabel.text = $"{CurrentCost}";
    }
}