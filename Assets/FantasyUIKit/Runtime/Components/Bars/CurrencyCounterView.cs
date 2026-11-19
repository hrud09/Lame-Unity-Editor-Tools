using System;
using UnityEngine;
using UnityEngine.UI;
using FantasyUI.Animation;
using FantasyUI.Data;

namespace FantasyUI.Components
{
    public enum CurrencyKind { Gold, Gems }

    /// <summary>
    /// Top HUD Currency Bar (Gold Coins or Cyan Gems with optional purple '+' bookmark button).
    /// Auto-binds to UIEventChannels when assigned.
    /// </summary>
    public class CurrencyCounterView : MonoBehaviour
    {
        [SerializeField] private CurrencyKind currencyKind = CurrencyKind.Gold;
        [SerializeField] private UIEventChannels eventChannel;
        [SerializeField] private UINumberRoller numberRoller;
        [SerializeField] private Button plusButton;

        public event Action OnPlusClicked;

        private void Awake()
        {
            if (plusButton != null)
                plusButton.onClick.AddListener(() => OnPlusClicked?.Invoke());
        }

        private void OnEnable()
        {
            if (eventChannel == null) return;
            if (currencyKind == CurrencyKind.Gold) eventChannel.OnGoldChanged += HandleChanged;
            else eventChannel.OnGemsChanged += HandleChanged;
        }

        private void OnDisable()
        {
            if (eventChannel == null) return;
            if (currencyKind == CurrencyKind.Gold) eventChannel.OnGoldChanged -= HandleChanged;
            else eventChannel.OnGemsChanged -= HandleChanged;
        }

        public void BindReferences(CurrencyKind kind, UINumberRoller roller, Button plusBtn = null, UIEventChannels channel = null)
        {
            currencyKind = kind;
            numberRoller = roller;
            plusButton = plusBtn;
            eventChannel = channel;
        }

        public void SetAmount(long amount, bool instant = false)
        {
            if (numberRoller != null) numberRoller.SetValue(amount, instant);
        }

        private void HandleChanged(long val) => SetAmount(val, false);
    }
}
