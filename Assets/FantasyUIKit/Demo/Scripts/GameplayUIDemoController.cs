using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using FantasyUI.Core;
using FantasyUI.Components;
using FantasyUI.Data;
using FantasyUI.Theme;

namespace FantasyUI.Demo
{
    /// <summary>
    /// Live interactive runtime sandbox driving every single HUD element and Modal inside GameplayScene.
    /// Drop-in example showing how game code binds cleanly to Fantasy UI via UIEventChannels & modular controllers.
    /// </summary>
    public class GameplayUIDemoController : MonoBehaviour
    {
        [Header("Event Bus & Theme")]
        [SerializeField] private UIEventChannels eventChannel;
        [SerializeField] private UIThemeManager themeManager;

        [Header("HUD References")]
        [SerializeField] private CurrencyCounterView goldCounter;
        [SerializeField] private CurrencyCounterView gemCounter;
        [SerializeField] private TMP_Text timerText;
        [SerializeField] private FantasyProgressBar profileHealthBar;
        [SerializeField] private DualPlayerVersusBar versusBar;
        [SerializeField] private VirtualJoystick joystick;
        [SerializeField] private WeaponSlotView[] weaponSlots;
        [SerializeField] private MinimapController minimap;
        [SerializeField] private QuestRailController questRail;
        [SerializeField] private TMP_Text inventoryBadgeText;
        [SerializeField] private Button inventoryTileButton;
        [SerializeField] private Button buildShipTileButton;
        [SerializeField] private Button pauseTileButton;
        [SerializeField] private Button weaponModeToggleButton;

        [Header("Modal References")]
        [SerializeField] private PauseScrollModal pauseScrollModal;
        [SerializeField] private LevelCompleteModal levelCompleteModal;
        [SerializeField] private LeatherInventoryModal leatherInventoryModal;
        [SerializeField] private DiscoveryPopupModal discoveryModal;
        [SerializeField] private ShipUpgradeModal shipUpgradeModal;
        [SerializeField] private PowerupSelectionModal powerupModal;

        [Header("Feedback References")]
        [SerializeField] private CelebrationBannerView celebrationBanner;
        [SerializeField] private ToastNotificationStack toastStack;

        [Header("Demo Icons")]
        [SerializeField] private Sprite iconSword;
        [SerializeField] private Sprite iconBow;
        [SerializeField] private Sprite iconCannon;
        [SerializeField] private Sprite iconPotion;
        [SerializeField] private Sprite iconFish;
        [SerializeField] private Sprite iconBoat;
        [SerializeField] private Sprite iconShield;
        [SerializeField] private Sprite iconTarget;
        [SerializeField] private Sprite iconLightning;
        [SerializeField] private Sprite iconGem;
        [SerializeField] private Sprite avatar1;
        [SerializeField] private Sprite avatar2;
        [SerializeField] private Sprite avatar3;

        private long _gold = 19456000;
        private long _gems = 19456;
        private float _hp = 2000f;
        private float _maxHp = 5000f;
        private float _mp = 19456000f;
        private float _oppHp = 19456000f;
        private float _timerSeconds = 10f * 60f + 56f;
        private int _shipLevel = 4;
        private float _heading;
        private float _patrolAngle;
        private bool _combatModeMelee;

        private readonly List<InventoryItemData> _inventory = new List<InventoryItemData>();
        private readonly List<StatEntryData> _shipStats = new List<StatEntryData>();

        private void Start()
        {
            SeedMockData();
            WireButtons();
            PushInitialState();
        }

        private void Update()
        {
            // Live countdown timer (10:56)
            _timerSeconds = Mathf.Max(0f, _timerSeconds - Time.deltaTime);
            if (timerText != null)
            {
                int mins = Mathf.FloorToInt(_timerSeconds / 60f);
                int secs = Mathf.FloorToInt(_timerSeconds % 60f);
                timerText.text = $"{mins:00}:{secs:00}";
            }

            // Rotate minimap player heading with joystick or gentle drift
            if (joystick != null && joystick.InputVector.sqrMagnitude > 0.01f)
            {
                _heading = Mathf.Atan2(joystick.Horizontal, joystick.Vertical) * Mathf.Rad2Deg;
                minimap?.SetPlayerHeading(_heading);
            }

            // Animate a patrolling enemy marker on the minimap
            _patrolAngle += Time.deltaTime * 0.55f;
            minimap?.UpsertMarker(new MinimapMarkerData
            {
                id = "EnemyRaider",
                normalizedMapPos = new Vector2(Mathf.Cos(_patrolAngle) * 0.58f, Mathf.Sin(_patrolAngle) * 0.58f),
                kind = MinimapMarkerKind.BossSkull
            });
        }

        private void SeedMockData()
        {
            _inventory.Clear();
            _inventory.Add(new InventoryItemData { id = "dagger", displayName = "Iron Cutlass", description = "Forged in dungeon steel. +35 Attack Power.", category = InventoryCategory.Weapons, rarity = ItemRarity.Rare, icon = iconSword, count = 1, sellValue = 1200, actionLabel = "EQUIP" });
            _inventory.Add(new InventoryItemData { id = "bow", displayName = "Hunter Recurve Bow", description = "Hand-carved ash longbow with razor fletching.", category = InventoryCategory.Weapons, rarity = ItemRarity.Epic, icon = iconBow, count = 1, sellValue = 2800, actionLabel = "EQUIP" });
            _inventory.Add(new InventoryItemData { id = "koi", displayName = "Azure Koi", description = "Rare glowing lagoon fish prized by harbor merchants.", category = InventoryCategory.FishBag, rarity = ItemRarity.Legendary, icon = iconFish, count = 3, sellValue = 4500, actionLabel = "SELL" });
            _inventory.Add(new InventoryItemData { id = "potion", displayName = "Crimson Elixir", description = "Restores 1,500 HP instantly in combat.", category = InventoryCategory.PotionBag, rarity = ItemRarity.Uncommon, icon = iconPotion, count = 5, sellValue = 350, actionLabel = "DRINK" });
            _inventory.Add(new InventoryItemData { id = "shield", displayName = "Aegis Buckler", description = "Riveted iron shield reducing cannon splash by 20%.", category = InventoryCategory.Hardware, rarity = ItemRarity.Rare, icon = iconShield, count = 2, sellValue = 1600, actionLabel = "EQUIP" });

            _shipStats.Clear();
            _shipStats.Add(new StatEntryData { statName = "HULL ARMOR", valueText = "4,200", normalizedFill = 0.72f, upgradeCost = 2500 });
            _shipStats.Add(new StatEntryData { statName = "CANNON VOLLEY", valueText = "840 DMG", normalizedFill = 0.60f, upgradeCost = 3200 });
            _shipStats.Add(new StatEntryData { statName = "SAIL SPEED", valueText = "18.5 KN", normalizedFill = 0.80f, upgradeCost = 1800 });
        }

        private void WireButtons()
        {
            if (inventoryTileButton != null) inventoryTileButton.onClick.AddListener(OpenInventoryModal);
            if (buildShipTileButton != null) buildShipTileButton.onClick.AddListener(OpenShipUpgradeModal);
            if (pauseTileButton != null) pauseTileButton.onClick.AddListener(OpenPauseModal);
            if (weaponModeToggleButton != null) weaponModeToggleButton.onClick.AddListener(ToggleWeaponMode);

            if (goldCounter != null) goldCounter.OnPlusClicked += () => AddGold(25000);
            if (gemCounter != null) gemCounter.OnPlusClicked += () => AddGems(500);

            if (questRail != null)
            {
                questRail.OnQuestClicked += q =>
                {
                    if (q.status == QuestStatus.CollectNow)
                    {
                        AddGold(q.rewardGold);
                        toastStack?.ShowToast($"COLLECTED +{q.rewardGold:N0} GOLD!", QuestStatus.CollectNow);
                    }
                    else
                    {
                        toastStack?.ShowToast(q.title, q.status);
                    }
                };
            }

            if (pauseScrollModal != null)
            {
                pauseScrollModal.OnExtraLifeClicked += () =>
                {
                    if (_gold >= 10500)
                    {
                        AddGold(-10500);
                        HealPlayer(1500f);
                        toastStack?.ShowToast("EXTRA LIFE CLAIMED!", QuestStatus.CollectNow);
                    }
                };
                pauseScrollModal.OnRestartClicked += () =>
                {
                    _hp = _maxHp;
                    PushHealth();
                    pauseScrollModal.CloseSelf();
                    toastStack?.ShowToast("STAGE RESTARTED!", QuestStatus.InProgress);
                };
                pauseScrollModal.OnQuitClicked += () =>
                {
                    pauseScrollModal.CloseSelf();
                    celebrationBanner?.Play(CelebrationType.Defeated);
                };
            }

            if (leatherInventoryModal != null)
            {
                leatherInventoryModal.OnItemActionClicked += item =>
                {
                    if (item.category == InventoryCategory.PotionBag) HealPlayer(1500f);
                    else if (item.category == InventoryCategory.FishBag) AddGold(item.sellValue);
                    toastStack?.ShowToast($"{item.actionLabel}: {item.displayName}", QuestStatus.CollectNow);
                };
                leatherInventoryModal.OnSellAllClicked += () =>
                {
                    long total = 0;
                    foreach (var it in _inventory) total += it.sellValue * it.count;
                    AddGold(total);
                    toastStack?.ShowToast($"SOLD LOOT +{total:N0}G!", QuestStatus.CollectNow);
                };
            }

            if (shipUpgradeModal != null)
            {
                shipUpgradeModal.OnStatUpgradeClicked += st =>
                {
                    if (_gold < st.upgradeCost) return;
                    AddGold(-st.upgradeCost);
                    st.normalizedFill = Mathf.Min(1f, st.normalizedFill + 0.08f);
                    st.upgradeCost = Mathf.RoundToInt(st.upgradeCost * 1.25f);
                    _shipLevel++;
                    shipUpgradeModal.Present("SHIP BUILDING", _shipLevel, iconBoat, _shipStats);
                    toastStack?.ShowToast($"{st.statName} UPGRADED!", QuestStatus.CollectNow);
                };
            }

            if (powerupModal != null)
            {
                powerupModal.OnPowerupPicked += p =>
                {
                    if (p.cost > 0) AddGold(-p.cost);
                    toastStack?.ShowToast($"EQUIPPED {p.title}!", QuestStatus.CollectNow);
                };
            }
        }

        private void PushInitialState()
        {
            goldCounter?.SetAmount(_gold, instant: true);
            gemCounter?.SetAmount(_gems, instant: true);
            versusBar?.SetPlayers("PLAYER12", avatar1, "PLAYER144", avatar2);
            PushHealth();
            RefreshInventoryBadge();

            // Weapon Slots
            var weaponDefs = new[]
            {
                new WeaponSlotData { weaponId = "cannon", weaponName = "CANNON", icon = iconCannon, cooldownDuration = 2.0f },
                new WeaponSlotData { weaponId = "sword",  weaponName = "SLASH",  icon = iconSword,  cooldownDuration = 1.5f },
                new WeaponSlotData { weaponId = "bow",    weaponName = "VOLLEY", icon = iconBow,    cooldownDuration = 3.5f },
                new WeaponSlotData { weaponId = "shield", weaponName = "AEGIS",  icon = iconShield, cooldownDuration = 5.0f },
            };
            if (weaponSlots != null)
            {
                for (int i = 0; i < weaponSlots.Length && i < weaponDefs.Length; i++)
                    weaponSlots[i]?.Configure(weaponDefs[i]);
            }

            // Minimap static landmarks
            if (minimap != null)
            {
                minimap.UpsertMarker(new MinimapMarkerData { id = "ShipGarage", normalizedMapPos = new Vector2(-0.42f, 0.35f), kind = MinimapMarkerKind.FriendlyTower });
                minimap.UpsertMarker(new MinimapMarkerData { id = "FishMarket", normalizedMapPos = new Vector2(0.45f, 0.38f), kind = MinimapMarkerKind.Shield });
                minimap.UpsertMarker(new MinimapMarkerData { id = "Treasure",   normalizedMapPos = new Vector2(0.32f, -0.45f), kind = MinimapMarkerKind.GoldCrystal });
                minimap.UpsertMarker(new MinimapMarkerData { id = "Outpost",    normalizedMapPos = new Vector2(-0.48f, -0.36f), kind = MinimapMarkerKind.EnemyTower });
            }

            // Quests Rail (Ref 1)
            questRail?.SetQuests(new List<QuestItemData>
            {
                new QuestItemData { id = "q1", title = "SINK 5 RAIDER SHIPS", icon = iconSword,  status = QuestStatus.InProgress, rewardGold = 5000 },
                new QuestItemData { id = "q2", title = "HARBOR SPRINT BONUS", icon = iconShield, status = QuestStatus.TimeIsUp,   rewardGold = 2500 },
                new QuestItemData { id = "q3", title = "BULLSEYE BOUNTY",     icon = iconTarget, status = QuestStatus.CollectNow, rewardGold = 15000 },
            });
        }

        public void AddGold(long delta)
        {
            _gold = System.Math.Max(0, _gold + delta);
            goldCounter?.SetAmount(_gold);
            leatherInventoryModal?.SetGold(_gold);
            eventChannel?.RaiseGold(_gold);
        }

        public void AddGems(long delta)
        {
            _gems = System.Math.Max(0, _gems + delta);
            gemCounter?.SetAmount(_gems);
            eventChannel?.RaiseGems(_gems);
        }

        public void DamagePlayer(float amount = 450f)
        {
            _hp = Mathf.Max(0f, _hp - amount);
            PushHealth();
        }

        public void HealPlayer(float amount = 900f)
        {
            _hp = Mathf.Min(_maxHp, _hp + amount);
            PushHealth();
        }

        private void PushHealth()
        {
            profileHealthBar?.SetValue(_hp, _maxHp);
            eventChannel?.RaisePlayerHealth(_hp, _maxHp);
            eventChannel?.RaisePlayerMana(_mp, 20000000f);
            eventChannel?.RaiseOpponentHealth(_oppHp, 20000000f);
            eventChannel?.RaiseOpponentMana(_oppHp, 20000000f);
        }

        private void RefreshInventoryBadge()
        {
            if (inventoryBadgeText != null)
                inventoryBadgeText.text = $"{_inventory.Count}/20";
        }

        private void ToggleWeaponMode()
        {
            _combatModeMelee = !_combatModeMelee;
            toastStack?.ShowToast(_combatModeMelee ? "MODE: MELEE ASSAULT" : "MODE: CANNON BROADSIDE", QuestStatus.InProgress);
        }

        // === Public Triggers for Demo Sandbox Toolbar & Gameplay Calls ===
        public void OpenPauseModal()
        {
            if (pauseScrollModal == null) return;
            pauseScrollModal.Configure("TITLE EXAMPLE", 10500);
            UIManager.Instance?.OpenModal(pauseScrollModal, closeOthers: true);
        }

        public void OpenLevelCompleteModal()
        {
            if (levelCompleteModal == null) return;
            var board = new List<LeaderboardEntryData>
            {
                new LeaderboardEntryData { rank = 1, playerName = "MELISSA", score = 20500, avatar = avatar1 },
                new LeaderboardEntryData { rank = 2, playerName = "KEVIN",   score = 20000, avatar = avatar2 },
                new LeaderboardEntryData { rank = 3, playerName = "RUBY",    score = 10500, avatar = avatar3 },
            };
            levelCompleteModal.Present("MENU EXAMPLE", "LEVEL COMPLETE!", "EARN 1000 POINTS AND ALL 3 STARS.", 2, board);
            UIManager.Instance?.OpenModal(levelCompleteModal, closeOthers: true);
        }

        public void OpenInventoryModal()
        {
            if (leatherInventoryModal == null) return;
            leatherInventoryModal.SetInventory(_inventory, 20, _gold);
            UIManager.Instance?.OpenModal(leatherInventoryModal, closeOthers: true);
        }

        public void OpenDiscoveryModal()
        {
            if (discoveryModal == null) return;
            discoveryModal.Present("NEW DISCOVERY!", "AZURE GOLDEN KOI", "A legendary bioluminescent reef fish found only near sunken citadels.", iconFish, new List<StatEntryData>
            {
                new StatEntryData { statName = "WEIGHT", valueText = "14.8 KG", icon = iconShield },
                new StatEntryData { statName = "VALUE",  valueText = "4,500 G", icon = iconGem },
                new StatEntryData { statName = "RARITY", valueText = "LEGENDARY", icon = iconLightning },
            });
            UIManager.Instance?.OpenModal(discoveryModal, closeOthers: true);
        }

        public void OpenShipUpgradeModal()
        {
            if (shipUpgradeModal == null) return;
            shipUpgradeModal.Present("SHIP BUILDING", _shipLevel, iconBoat, _shipStats);
            UIManager.Instance?.OpenModal(shipUpgradeModal, closeOthers: true);
        }

        public void OpenPowerupModal()
        {
            if (powerupModal == null) return;
            powerupModal.Present(new List<PowerupCardData>
            {
                new PowerupCardData { id = "p1", title = "STORM VOLLEY", description = "+40% Cannon Fire Rate", icon = iconCannon, cost = 1000, buttonLabel = "PICK" },
                new PowerupCardData { id = "p2", title = "IRON AEGIS",   description = "+1,500 Max Hull HP",    icon = iconShield, cost = 1500, buttonLabel = "PICK" },
                new PowerupCardData { id = "p3", title = "ARCANE BOLT",  description = "Chain Lightning Crits", icon = iconLightning, cost = 0, buttonLabel = "FREE" },
            });
            UIManager.Instance?.OpenModal(powerupModal, closeOthers: true);
        }

        public void PlayVictoryBanner()  => celebrationBanner?.Play(CelebrationType.Victory);
        public void PlayDefeatedBanner() => celebrationBanner?.Play(CelebrationType.Defeated);
        public void PlayAmazingBanner()  => celebrationBanner?.Play(CelebrationType.Amazing);
        public void PlayAwesomeBanner()  => celebrationBanner?.Play(CelebrationType.Awesome);
        public void CycleTheme()         => themeManager?.CycleNextTheme();

        public void BindAll(
            UIEventChannels channel, UIThemeManager tm,
            CurrencyCounterView gold, CurrencyCounterView gem, TMP_Text timer, FantasyProgressBar profileHp,
            DualPlayerVersusBar versus, VirtualJoystick joy, WeaponSlotView[] weapons, MinimapController map,
            QuestRailController quests, TMP_Text invBadge, Button invBtn, Button buildBtn, Button pauseBtn, Button modeBtn,
            PauseScrollModal pause, LevelCompleteModal lvlComp, LeatherInventoryModal invModal,
            DiscoveryPopupModal discModal, ShipUpgradeModal shipModal, PowerupSelectionModal pwrModal,
            CelebrationBannerView celeb, ToastNotificationStack toast,
            Sprite sSword, Sprite sBow, Sprite sCannon, Sprite sPotion, Sprite sFish, Sprite sBoat,
            Sprite sShield, Sprite sTarget, Sprite sBolt, Sprite sGem, Sprite a1, Sprite a2, Sprite a3)
        {
            eventChannel = channel; themeManager = tm;
            goldCounter = gold; gemCounter = gem; timerText = timer; profileHealthBar = profileHp;
            versusBar = versus; joystick = joy; weaponSlots = weapons; minimap = map;
            questRail = quests; inventoryBadgeText = invBadge; inventoryTileButton = invBtn;
            buildShipTileButton = buildBtn; pauseTileButton = pauseBtn; weaponModeToggleButton = modeBtn;
            pauseScrollModal = pause; levelCompleteModal = lvlComp; leatherInventoryModal = invModal;
            discoveryModal = discModal; shipUpgradeModal = shipModal; powerupModal = pwrModal;
            celebrationBanner = celeb; toastStack = toast;
            iconSword = sSword; iconBow = sBow; iconCannon = sCannon; iconPotion = sPotion;
            iconFish = sFish; iconBoat = sBoat; iconShield = sShield; iconTarget = sTarget;
            iconLightning = sBolt; iconGem = sGem; avatar1 = a1; avatar2 = a2; avatar3 = a3;
        }
    }
}
