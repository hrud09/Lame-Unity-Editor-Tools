using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using FantasyUI.Core;
using FantasyUI.Data;
using FantasyUI.Theme;

namespace FantasyUI.Components
{
    /// <summary>
    /// Dark-Fantasy New Fish / New Loot Discovery Popup (upgraded from GameplayScene's New Fish Pop Up).
    /// </summary>
    public class DiscoveryPopupModal : UIModal
    {
        [SerializeField] private TMP_Text headerTitle;
        [SerializeField] private TMP_Text itemNameText;
        [SerializeField] private TMP_Text descriptionText;
        [SerializeField] private Image itemIconImage;
        [SerializeField] private Transform statsRoot;
        [SerializeField] private GameObject statRowTemplate;

        private readonly List<GameObject> _rows = new List<GameObject>();

        public void BindReferences(TMP_Text header, TMP_Text itemName, TMP_Text desc, Image icon, Transform stats, GameObject rowTpl)
        {
            headerTitle = header; itemNameText = itemName; descriptionText = desc;
            itemIconImage = icon; statsRoot = stats; statRowTemplate = rowTpl;
        }

        public void Present(string title, string itemName, string description, Sprite icon, IList<StatEntryData> stats)
        {
            if (headerTitle != null) headerTitle.text = title;
            if (itemNameText != null) itemNameText.text = itemName;
            if (descriptionText != null) descriptionText.text = description;
            if (itemIconImage != null && icon != null) itemIconImage.sprite = icon;

            for (int i = 0; i < _rows.Count; i++)
                if (_rows[i] != null) Destroy(_rows[i]);
            _rows.Clear();

            if (stats == null || statsRoot == null || statRowTemplate == null) return;
            statRowTemplate.SetActive(false);

            for (int i = 0; i < stats.Count; i++)
            {
                var st = stats[i];
                var row = Instantiate(statRowTemplate, statsRoot);
                row.SetActive(true);
                _rows.Add(row);

                var nameTxt = row.transform.Find("NameText")?.GetComponent<TMP_Text>();
                var valTxt = row.transform.Find("ValueText")?.GetComponent<TMP_Text>();
                var iconImg = row.transform.Find("Icon")?.GetComponent<Image>();
                if (nameTxt != null) nameTxt.text = st.statName;
                if (valTxt != null) valTxt.text = st.valueText;
                if (iconImg != null && st.icon != null) iconImg.sprite = st.icon;
            }
        }
    }

    /// <summary>
    /// Dark-Fantasy Ship Building & Upgrade Workshop Modal (upgraded from GameplayScene's Ship Building Pop Up).
    /// </summary>
    public class ShipUpgradeModal : UIModal
    {
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text shipLevelText;
        [SerializeField] private Image shipPreviewImage;
        [SerializeField] private Transform statUpgradesRoot;
        [SerializeField] private GameObject statUpgradeTemplate;

        private readonly List<GameObject> _rows = new List<GameObject>();
        public event Action<StatEntryData> OnStatUpgradeClicked;

        public void BindReferences(TMP_Text title, TMP_Text levelTxt, Image preview, Transform root, GameObject tpl)
        {
            titleText = title; shipLevelText = levelTxt; shipPreviewImage = preview;
            statUpgradesRoot = root; statUpgradeTemplate = tpl;
        }

        public void Present(string shipTitle, int shipLevel, Sprite shipSprite, IList<StatEntryData> stats)
        {
            if (titleText != null) titleText.text = shipTitle;
            if (shipLevelText != null) shipLevelText.text = $"LVL {shipLevel}";
            if (shipPreviewImage != null && shipSprite != null) shipPreviewImage.sprite = shipSprite;

            for (int i = 0; i < _rows.Count; i++)
                if (_rows[i] != null) Destroy(_rows[i]);
            _rows.Clear();

            if (stats == null || statUpgradesRoot == null || statUpgradeTemplate == null) return;
            statUpgradeTemplate.SetActive(false);

            for (int i = 0; i < stats.Count; i++)
            {
                var st = stats[i];
                var row = Instantiate(statUpgradeTemplate, statUpgradesRoot);
                row.SetActive(true);
                _rows.Add(row);

                var nTxt = row.transform.Find("StatName")?.GetComponent<TMP_Text>();
                var vTxt = row.transform.Find("StatValue")?.GetComponent<TMP_Text>();
                var fill = row.transform.Find("BarFrame/Fill")?.GetComponent<Image>();
                var upBtn = row.transform.Find("UpgradeBtn")?.GetComponent<Button>();
                var costTxt = row.transform.Find("UpgradeBtn/CostText")?.GetComponent<TMP_Text>();

                if (nTxt != null) nTxt.text = st.statName;
                if (vTxt != null) vTxt.text = st.valueText;
                if (fill != null) fill.rectTransform.anchorMax = new Vector2(Mathf.Clamp01(st.normalizedFill), 1f);
                if (costTxt != null) costTxt.text = $"{st.upgradeCost:N0}";
                if (upBtn != null)
                {
                    var captured = st;
                    upBtn.onClick.AddListener(() =>
                    {
                        UIThemeManager.Instance?.PlaySfx(UISfxCue.CoinClink);
                        OnStatUpgradeClicked?.Invoke(captured);
                    });
                }
            }
        }
    }

    /// <summary>
    /// Dark-Fantasy Roguelike Powerup Card Selection Overlay (upgraded from GameplayScene's Powerup Selection Canvas).
    /// </summary>
    public class PowerupSelectionModal : UIModal
    {
        [SerializeField] private Transform cardsRoot;
        [SerializeField] private GameObject cardTemplate;

        private readonly List<GameObject> _cards = new List<GameObject>();
        public event Action<PowerupCardData> OnPowerupPicked;

        public void BindReferences(Transform root, GameObject template)
        {
            cardsRoot = root;
            cardTemplate = template;
        }

        public void Present(IList<PowerupCardData> powerups)
        {
            for (int i = 0; i < _cards.Count; i++)
                if (_cards[i] != null) Destroy(_cards[i]);
            _cards.Clear();

            if (powerups == null || cardsRoot == null || cardTemplate == null) return;
            cardTemplate.SetActive(false);

            for (int i = 0; i < powerups.Count; i++)
            {
                var p = powerups[i];
                var card = Instantiate(cardTemplate, cardsRoot);
                card.name = $"PowerupCard_{p.id}";
                card.SetActive(true);
                _cards.Add(card);

                var titleTxt = card.transform.Find("TitleRibbon/TitleText")?.GetComponent<TMP_Text>();
                var iconImg = card.transform.Find("IconPedestal/Icon")?.GetComponent<Image>();
                var descTxt = card.transform.Find("DescText")?.GetComponent<TMP_Text>();
                var costTxt = card.transform.Find("CostPill/CostText")?.GetComponent<TMP_Text>();
                var pickBtn = card.transform.Find("ActionButton")?.GetComponent<Button>();
                var pickTxt = card.transform.Find("ActionButton/Label")?.GetComponent<TMP_Text>();

                if (titleTxt != null) titleTxt.text = p.title;
                if (iconImg != null && p.icon != null) iconImg.sprite = p.icon;
                if (descTxt != null) descTxt.text = p.description;
                if (costTxt != null) costTxt.text = p.cost > 0 ? p.cost.ToString("N0") : "FREE";
                if (pickTxt != null) pickTxt.text = p.buttonLabel;
                if (pickBtn != null)
                {
                    var captured = p;
                    pickBtn.onClick.AddListener(() =>
                    {
                        UIThemeManager.Instance?.PlaySfx(UISfxCue.Equip);
                        OnPowerupPicked?.Invoke(captured);
                        CloseSelf();
                    });
                }
            }
        }
    }
}
