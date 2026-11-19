using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using FantasyUI.Animation;
using FantasyUI.Core;
using FantasyUI.Data;
using FantasyUI.Theme;

namespace FantasyUI.Components
{
    /// <summary>
    /// Exact Reference Image 4 Faceted Stitched Plum-Leather Inventory Bag Modal,
    /// seamlessly unified with all GameplayScene Inventory capabilities (category tabs, 2x5+ octagonal slot grid,
    /// selection highlight, bag capacity bar, item detail card, USE & SELL ALL buttons, and bottom stacked Gold counter).
    /// </summary>
    public class LeatherInventoryModal : UIModal
    {
        [Header("Bag Header & Gold")]
        [SerializeField] private TMP_Text beltTitleText;
        [SerializeField] private UINumberRoller goldRoller;
        [SerializeField] private FantasyProgressBar capacityBar;

        [Header("Category Tabs")]
        [SerializeField] private Button tabAllButton;
        [SerializeField] private Button tabFishButton;
        [SerializeField] private Button tabWeaponsButton;
        [SerializeField] private Button tabPotionsButton;

        [Header("Octagonal Slot Grid")]
        [SerializeField] private Transform gridRoot;
        [SerializeField] private GameObject slotTemplate;
        [SerializeField] private GameObject emptyGridLabel;
        [SerializeField] private Sprite slotNormalSprite;
        [SerializeField] private Sprite slotSelectedSprite;
        [SerializeField] private int minimumVisibleSlots = 10; // 2x5 Ref 4 layout

        [Header("Item Detail Card")]
        [SerializeField] private GameObject detailContentRoot;
        [SerializeField] private GameObject detailEmptyRoot;
        [SerializeField] private Image detailIcon;
        [SerializeField] private TMP_Text detailName;
        [SerializeField] private TMP_Text detailRarity;
        [SerializeField] private TMP_Text detailDescription;
        [SerializeField] private TMP_Text detailCountInfo;
        [SerializeField] private Button detailActionButton;
        [SerializeField] private TMP_Text detailActionLabel;
        [SerializeField] private Button sellAllButton;

        private readonly List<InventoryItemData> _allItems = new List<InventoryItemData>();
        private readonly List<GameObject> _slotObjects = new List<GameObject>();
        private InventoryCategory _activeCategory = InventoryCategory.All;
        private InventoryItemData _selectedItem;
        private int _maxCapacity = 20;

        public event Action<InventoryItemData> OnItemActionClicked;
        public event Action OnSellAllClicked;

        protected override void Awake()
        {
            base.Awake();
            if (tabAllButton != null) tabAllButton.onClick.AddListener(() => SelectCategory(InventoryCategory.All, "BAG 1"));
            if (tabFishButton != null) tabFishButton.onClick.AddListener(() => SelectCategory(InventoryCategory.FishBag, "FISH BAG"));
            if (tabWeaponsButton != null) tabWeaponsButton.onClick.AddListener(() => SelectCategory(InventoryCategory.Weapons, "WEAPONS"));
            if (tabPotionsButton != null) tabPotionsButton.onClick.AddListener(() => SelectCategory(InventoryCategory.PotionBag, "POTIONS"));
            if (detailActionButton != null) detailActionButton.onClick.AddListener(() => { if (_selectedItem != null) OnItemActionClicked?.Invoke(_selectedItem); });
            if (sellAllButton != null) sellAllButton.onClick.AddListener(() => OnSellAllClicked?.Invoke());
        }

        public void BindReferences(
            TMP_Text beltTitle, UINumberRoller gold, FantasyProgressBar capBar,
            Button tAll, Button tFish, Button tWeapons, Button tPotions,
            Transform grid, GameObject template, GameObject emptyLbl, Sprite normalSlot, Sprite selSlot,
            GameObject detContent, GameObject detEmpty, Image detIcon, TMP_Text detName, TMP_Text detRarity,
            TMP_Text detDesc, TMP_Text detInfo, Button detActBtn, TMP_Text detActLbl, Button sellBtn)
        {
            beltTitleText = beltTitle; goldRoller = gold; capacityBar = capBar;
            tabAllButton = tAll; tabFishButton = tFish; tabWeaponsButton = tWeapons; tabPotionsButton = tPotions;
            gridRoot = grid; slotTemplate = template; emptyGridLabel = emptyLbl;
            slotNormalSprite = normalSlot; slotSelectedSprite = selSlot;
            detailContentRoot = detContent; detailEmptyRoot = detEmpty; detailIcon = detIcon;
            detailName = detName; detailRarity = detRarity; detailDescription = detDesc;
            detailCountInfo = detInfo; detailActionButton = detActBtn; detailActionLabel = detActLbl;
            sellAllButton = sellBtn;
        }

        public void SetInventory(IList<InventoryItemData> items, int maxCapacity, long currentGold)
        {
            _allItems.Clear();
            if (items != null) _allItems.AddRange(items);
            _maxCapacity = Mathf.Max(1, maxCapacity);
            if (goldRoller != null) goldRoller.SetValue(currentGold, instant: !gameObject.activeInHierarchy);
            if (capacityBar != null) capacityBar.SetValue(_allItems.Count, _maxCapacity, instant: true);
            RebuildGrid();
        }

        public void SetGold(long currentGold) => goldRoller?.SetValue(currentGold);

        public void SelectCategory(InventoryCategory category, string bagTitle)
        {
            _activeCategory = category;
            if (beltTitleText != null) beltTitleText.text = bagTitle;
            _selectedItem = null;
            UIThemeManager.Instance?.PlaySfx(UISfxCue.LeatherOpen);
            RebuildGrid();
        }

        private void RebuildGrid()
        {
            for (int i = 0; i < _slotObjects.Count; i++)
                if (_slotObjects[i] != null) Destroy(_slotObjects[i]);
            _slotObjects.Clear();

            if (gridRoot == null || slotTemplate == null) return;
            slotTemplate.SetActive(false);

            var filtered = new List<InventoryItemData>();
            for (int i = 0; i < _allItems.Count; i++)
            {
                if (_activeCategory == InventoryCategory.All || _allItems[i].category == _activeCategory)
                    filtered.Add(_allItems[i]);
            }

            if (emptyGridLabel != null) emptyGridLabel.SetActive(filtered.Count == 0);
            int totalSlots = Mathf.Max(minimumVisibleSlots, filtered.Count);

            for (int i = 0; i < totalSlots; i++)
            {
                var slotGo = Instantiate(slotTemplate, gridRoot);
                slotGo.name = $"BagSlot_{i:00}";
                slotGo.SetActive(true);
                _slotObjects.Add(slotGo);

                var bg = slotGo.GetComponent<Image>();
                var iconImg = slotGo.transform.Find("Icon")?.GetComponent<Image>();
                var countTxt = slotGo.transform.Find("Count")?.GetComponent<TMP_Text>();
                var rarityDot = slotGo.transform.Find("RarityStripe")?.GetComponent<Image>();
                var btn = slotGo.GetComponent<Button>();

                if (i < filtered.Count)
                {
                    var item = filtered[i];
                    bool isSel = item == _selectedItem;
                    if (bg != null) bg.sprite = isSel && slotSelectedSprite != null ? slotSelectedSprite : slotNormalSprite;
                    if (iconImg != null) { iconImg.enabled = true; iconImg.sprite = item.icon; }
                    if (countTxt != null) { countTxt.gameObject.SetActive(item.count > 1); countTxt.text = $"x{item.count}"; }
                    if (rarityDot != null) { rarityDot.enabled = true; rarityDot.color = RarityColor(item.rarity); }
                    if (btn != null)
                    {
                        var captured = item;
                        btn.interactable = true;
                        btn.onClick.AddListener(() => SelectItem(captured));
                    }
                }
                else
                {
                    if (bg != null) bg.sprite = slotNormalSprite;
                    if (iconImg != null) iconImg.enabled = false;
                    if (countTxt != null) countTxt.gameObject.SetActive(false);
                    if (rarityDot != null) rarityDot.enabled = false;
                    if (btn != null) btn.interactable = false;
                }
            }

            if (_selectedItem == null && filtered.Count > 0)
                _selectedItem = filtered[0];

            RefreshDetailCard();
        }

        private void SelectItem(InventoryItemData item)
        {
            _selectedItem = item;
            UIThemeManager.Instance?.PlaySfx(UISfxCue.Click);
            RebuildGrid();
        }

        private void RefreshDetailCard()
        {
            bool hasItem = _selectedItem != null;
            if (detailContentRoot != null) detailContentRoot.SetActive(hasItem);
            if (detailEmptyRoot != null) detailEmptyRoot.SetActive(!hasItem);
            if (!hasItem) return;

            if (detailIcon != null) detailIcon.sprite = _selectedItem.icon;
            if (detailName != null) detailName.text = _selectedItem.displayName;
            if (detailRarity != null)
            {
                detailRarity.text = _selectedItem.rarity.ToString().ToUpperInvariant();
                detailRarity.color = RarityColor(_selectedItem.rarity);
            }
            if (detailDescription != null) detailDescription.text = _selectedItem.description;
            if (detailCountInfo != null) detailCountInfo.text = $"Owned: {_selectedItem.count}  |  Value: {_selectedItem.sellValue:N0}g";
            if (detailActionLabel != null) detailActionLabel.text = _selectedItem.actionLabel;
        }

        public static Color RarityColor(ItemRarity rarity)
        {
            return rarity switch
            {
                ItemRarity.Uncommon  => new Color32(66, 194, 158, 255),
                ItemRarity.Rare      => new Color32(78, 203, 245, 255),
                ItemRarity.Epic      => new Color32(154, 102, 240, 255),
                ItemRarity.Legendary => new Color32(255, 212, 56, 255),
                _                    => new Color32(218, 196, 154, 255)
            };
        }
    }
}
