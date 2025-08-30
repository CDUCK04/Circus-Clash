using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UnitShopItem : MonoBehaviour
{
    [Header("Cost")]
    public int cost = 50;

    [Header("Optional UI")]
    public TMP_Text priceLabel;   // e.g., a small price text on the button

    Button _btn;
    bool _subscribed;

    void Awake()
    {
        _btn = GetComponent<Button>();
    }

    void OnEnable()
    {
        TrySubscribe();
        RefreshInteractable();
        if (priceLabel) priceLabel.text = cost.ToString();
    }

    void OnDisable()
    {
        TryUnsubscribe();
    }

    void TrySubscribe()
    {
        if (_subscribed) return;
        if (CurrencyManager.Instance != null)
        {
            CurrencyManager.Instance.OnTicketsChanged += HandleTicketsChanged;
            _subscribed = true;
        }
        else
        {
            // CurrencyManager not ready yet? Try again next frame
            Invoke(nameof(TrySubscribe), 0f);
        }
    }

    void TryUnsubscribe()
    {
        if (!_subscribed) return;
        if (CurrencyManager.Instance != null)
            CurrencyManager.Instance.OnTicketsChanged -= HandleTicketsChanged;
        _subscribed = false;
    }

    void HandleTicketsChanged(int _)
    {
        RefreshInteractable();
    }

    void RefreshInteractable()
    {
        if (_btn == null) return;

        bool canAfford = CurrencyManager.Instance != null && CurrencyManager.Instance.CanAfford(cost);
        _btn.interactable = canAfford;
    }

    // Hook this to the Button's OnClick
    public void Purchase()
    {
        if (CurrencyManager.Instance == null) return;

        if (CurrencyManager.Instance.Spend(cost))
        {
            // success – button states might change after spending
            RefreshInteractable();
        }
        else
        {
            // optional: feedback when unaffordable
        }
    }
}
