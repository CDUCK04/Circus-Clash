using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class TroopCounter : MonoBehaviour
{
    public static TroopCounter Instance;

    [Header("Limits")]
    public int maxPerSide = 20;

    [Header("UI (optional)")]
    public TMP_Text playerLable;

    public int playerCount;
    int enemyCount;

    void Awake()
    {
        if (Instance && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        UpdateUI();
    }

    public bool CanSpawnPlayer() => playerCount < maxPerSide;
    public bool CanSpawnEnemy() => enemyCount < maxPerSide;

    public bool Register(UnitBrain brain)
    {
        if (!brain) return false;
        bool sideFull = brain.isPlayerUnit ? (playerCount >= maxPerSide) : (enemyCount >= maxPerSide);
        if (sideFull) return false;

        if (brain.isPlayerUnit) playerCount++; else enemyCount++;
        var hp = brain.GetComponent<UnitHealth>();
        if (hp != null) hp.onDied.AddListener(() => Unregister(brain.isPlayerUnit));

        UpdateUI();
        return true;
    }

    public void Unregister(bool wasPlayerUnit)
    {
        if (wasPlayerUnit) playerCount = Mathf.Max(0, playerCount - 1);
        else enemyCount = Mathf.Max(0, enemyCount - 1);
        UpdateUI();
    }

    void UpdateUI()
    {
        
         playerLable.text = $"{playerCount}/{maxPerSide}";
    }
}
