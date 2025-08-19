using System;
using UnityEngine;
using TMPro;

public class CurrencyManager : MonoBehaviour
{
    public static CurrencyManager Instance;

    [Header("Starting & Income")]
    public int startingTickets = 100;
    public int ticketsPerTick = 5;
    public float tickSeconds = 1f;

    [Header("UI")]
    public TMP_Text ticketsText;

    public int Tickets { get; private set; }
    public event Action<int> OnTicketsChanged;

    void Awake()
    {
        // Make the singleton available as early as possible
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        // IMPORTANT: set tickets *before* any buttons enable
        Tickets = startingTickets;
        UpdateUI(); // fires OnTicketsChanged now, too
    }

    void Start()
    {
        // Start income after everything is alive
        InvokeRepeating(nameof(AddIncomeTick), tickSeconds, tickSeconds);
    }

    void AddIncomeTick()
    {
        Tickets += ticketsPerTick;
        UpdateUI();
    }

    public bool CanAfford(int cost) => Tickets >= cost;

    public bool Spend(int cost)
    {
        if (!CanAfford(cost)) return false;
        Tickets -= cost;
        UpdateUI();
        return true;
    }

    void UpdateUI()
    {
        if (ticketsText) ticketsText.text = $"Tickets: {Tickets}";
        OnTicketsChanged?.Invoke(Tickets);
    }
}
