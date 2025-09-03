using System;
using UnityEngine;
using TMPro;

public class CurrencyManager : MonoBehaviour
{
    public static CurrencyManager Instance;

    [Header("Starting & Income")]
    public int startingTickets = 100;
    public int ticketsPerTick = 2;
    public float tickSeconds = 1.5f;

    [Header("UI")]
    public TMP_Text ticketsText;

    public int Tickets { get; private set; }
    public event Action<int> OnTicketsChanged;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        Tickets = startingTickets;
        UpdateUI();
    }

    void Start()
    {
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
        if (ticketsText) ticketsText.text = $"{Tickets}";
        OnTicketsChanged?.Invoke(Tickets);
    }
}
